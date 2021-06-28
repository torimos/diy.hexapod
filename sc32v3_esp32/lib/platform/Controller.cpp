#include "Controller.h"
#include "IKSolver.h"
#include <math.h>
#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <errno.h>
#include <stdint.h>

// 5 LF ^ RF 0
// 4 LM + RM 1
// 3 LR . RR 2
static int ServoMap[] = {
	 8, 7, 6, 	//RR - tfc
	 5, 4, 3, 	//RM
	 2, 1, 0,  	//RF
	11,12,13, 	//LR
	14,15,16, 	//LM
	17,18,19,	//LF 
};
static int ServoInv[] = { 
	-1,-1,1,	//RR - tfc
	-1,-1,1,	//RM
	-1,-1,1,	//RF
	-1,-1,1,	//LR
	-1,-1,1,	//LM
	-1,-1,1		//LF 
};
static int ServoOffset[] =
{ 
	0,0,0, //10,-170,-30,	//RR - tfc
	0,0,0, //-20,-130,-40,	//RM
	0,0,0, //0,-20,0,		//RF

	0,0,0, //20,80,30,		//LR
	0,0,0, //70,220,-40,	//LM
	0,0,0, //-40,90,20		//LF 
};

Controller::Controller(InputDriver* a, ServoDriver* b, Stream* debugStream)
{
	sd = b;
	inputDrv = a;
	ik = new IKSolver();
	sw = new Stopwatch();
	model = new HexModel(HexConfig::LegsCount);
	debugSP = new SerialProtocol(debugStream);
}

Controller::~Controller()
{
	delete ik;
	delete sw;
	delete model;
}

void Controller::Setup()
{
	model->GaitsCount = HexConfig::GaitsCount;
	model->Gaits = new PhoenixGait[HexConfig::GaitsCount];
	model->Gaits[GaitType::Ripple12] =
	{
		.NomGaitSpeed = 70,
		.StepsInGait = 12,
		.NrLiftedPos = 3,
		.FrontDownPos = 2,
		.LiftDivFactor = 2,
		.TLDivFactor = 8,
		.HalfLiftHeight = 3,
		.GaitLegNr = { 7, 11, 3, 1, 5, 9 }
	};
	model->Gaits[GaitType::Tripod8] =
	{
		.NomGaitSpeed = 70,
		.StepsInGait = 8,
		.NrLiftedPos = 3,
		.FrontDownPos = 2,
		.LiftDivFactor = 2,
		.TLDivFactor = 4,
		.HalfLiftHeight = 3,
		.GaitLegNr = { 1, 5, 1, 5, 1, 5 }
	};
	model->Gaits[GaitType::TripleTripod12] =
	{
		.NomGaitSpeed = 50,
		.StepsInGait = 12,
		.NrLiftedPos = 3,
		.FrontDownPos = 2,
		.LiftDivFactor = 2,
		.TLDivFactor = 8,
		.HalfLiftHeight = 3,
		.GaitLegNr = { 5, 10, 3, 11, 4, 9 }
	};
	model->Gaits[GaitType::TripleTripod16] =
	{
		.NomGaitSpeed = 50,
		.StepsInGait = 16,
		.NrLiftedPos = 5,
		.FrontDownPos = 3,
		.LiftDivFactor = 4,
		.TLDivFactor = 10,
		.HalfLiftHeight = 1,
		.GaitLegNr = { 6, 13, 4, 14, 5, 12 }
	};
	model->Gaits[GaitType::Wave24] =
	{
		.NomGaitSpeed = 70,
		.StepsInGait = 24,
		.NrLiftedPos = 3,
		.FrontDownPos = 2,
		.LiftDivFactor = 2,
		.TLDivFactor = 20,
		.HalfLiftHeight = 3,
		.GaitLegNr = { 13, 17, 21, 1, 5, 9 }
	};
	model->Gaits[GaitType::Tripod6] =
	{
		.NomGaitSpeed = 70,
		.StepsInGait = 6,
		.NrLiftedPos = 2,
		.FrontDownPos = 1,
		.LiftDivFactor = 2,
		.TLDivFactor = 4,
		.HalfLiftHeight = 1,
		.GaitLegNr = { 1, 4, 1, 4, 1, 4 }
	};
	model->SelectedGaitType = GaitType::Tripod6;
	model->BalanceMode = false;
	model->LegLiftHeight = HexConfig::LegLiftHeight;
	model->ForceGaitStepCnt = 0;    // added to try to adjust starting positions depending on height...
	model->GaitStep = 1;
	model->gaitCur = &model->Gaits[model->SelectedGaitType];

	for (int i = 0; i < 6; i++)
	{
		model->LegsPos[i] = { HexConfig::DefaultLegsPosX[i], HexConfig::DefaultLegsPosY[i], HexConfig::DefaultLegsPosZ[i] };
	}

	model->PrevSelectedLeg = model->SelectedLeg = 0xFF; // No Leg selected
	model->Speed = 100;
	model->PowerOn = false;
	model->DebugOutput = true;
	
	sd->Init();
	sd->Reset();
	Log.printf("\033c");
	Log.printf("\033[%d;%dH", 0, 0);
}

long debugTimeout = 0;

bool Controller::Loop()
{
	unsigned long timeToWait = 0;
	unsigned long t = micros();
	sw->Restart();
	
	bool inputChanged = inputDrv->ProcessInput(model);
	
	if (inputChanged)
	{
		if (inputDrv->IsTerminate()) return false;
	}
	
	GPPlayer();
	SingleLegControl();
	GateSequence();
	Balance();
	SolveIKLegs();
	if (model->PowerOn)
	{
	    //Calculate Servo Move time
		if (model->ControlMode == ControlModeType::SingleLeg)
		{
			model->MoveTime = HexConfig::SingleLegControlDelay;
		}
		else
		{
			if ((fabs(model->TravelLength.x) > HexConfig::TravelDeadZone)
			  || (fabs(model->TravelLength.z) > HexConfig::TravelDeadZone)
			  || (fabs(model->TravelLength.y * 2) > HexConfig::TravelDeadZone))
			{
				model->MoveTime = (uint16_t)(model->gaitCur->NomGaitSpeed + (model->InputTimeDelay * 2) + model->Speed);

				//Add aditional delay when Balance mode is on
				if (model->BalanceMode)
					model->MoveTime = (uint16_t)(model->MoveTime + HexConfig::BalancingDelay);
			}
			else //Movement speed excl. Walking
				model->MoveTime = (uint16_t)(HexConfig::WalkingDelay + model->Speed);
		}

		UpdateServos(model->LegsAngle, model->MoveTime);

		for (int LegIndex = 0; LegIndex < HexConfig::LegsCount; LegIndex++)
		{
			if (((fabs(model->GaitPos[LegIndex].x) > HexConfig::GPlimit) ||
			    (fabs(model->GaitPos[LegIndex].z) > HexConfig::GPlimit) ||
			    (fabs(model->GaitRotY[LegIndex]) > HexConfig::GPlimit)))
			{
			    //For making sure that we are using timed move until all legs are down
				model->ExtraCycle = model->gaitCur->NrLiftedPos + 1;
				break;
			}
		}
		if (model->ExtraCycle > 0)
		{
			model->ExtraCycle--;
			model->Walking = !(model->ExtraCycle == 0);
			timeToWait = (model->PrevMoveTime - sw->GetElapsedMilliseconds());
			unsigned long time = millis() + timeToWait;
			do
			{
				inputDrv->ProcessInput(model);
			} while (millis() <= time);
		}
		CommitServos();
	}
	else
	{
		if (model->PrevPowerOn)
		{
			model->MoveTime = 600;
			UpdateServos(model->LegsAngle, model->MoveTime);
			CommitServos();
			delay(600);
		}
		else
		{
			sd->MoveAll(0);
			CommitServos();
		}
	}

	model->PrevControlMode = model->ControlMode;
	model->PrevMoveTime = model->MoveTime;
	model->PrevPowerOn = model->PowerOn;
	
	if (millis()>=debugTimeout)
	{
		if (model->DebugOutput)
		{
		//	Log.printf("\033c");
			Log.printf("\033[%d;%dH", 0, 0);
			model->Debug(1);
			inputDrv->Debug();
			Log.printf("timeToWait: %04ld\n\r", timeToWait);
			long t1 =  (long)micros() - t;
			Log.printf("Iteration Duration: %08ld\n\r", t1);
		}

		debugTimeout = millis()+50;
	}
	return true;
}

void Controller::GPPlayer()
{
	//todo:
}

void Controller::SingleLegControl()
{
	if (model->ControlMode != ControlModeType::SingleLeg) return;

	bool AllDown = (model->LegsPos[0].y == HexConfig::DefaultLegsPosY[0]) &&
	    (model->LegsPos[1].y == HexConfig::DefaultLegsPosY[1]) &&
	    (model->LegsPos[2].y == HexConfig::DefaultLegsPosY[2]) &&
	    (model->LegsPos[3].y == HexConfig::DefaultLegsPosY[3]) &&
	    (model->LegsPos[4].y == HexConfig::DefaultLegsPosY[4]) &&
	    (model->LegsPos[5].y == HexConfig::DefaultLegsPosY[5]);

	if (model->SelectedLeg < model->LegsCount)
	{
		if (model->SelectedLeg != model->PrevSelectedLeg)
		{
			if (AllDown)
			{
			    //Lift leg a bit when it got selected
				model->LegsPos[model->SelectedLeg].y = HexConfig::DefaultLegsPosY[model->SelectedLeg] - 30;
				//Store current status
				model->PrevSelectedLeg = model->SelectedLeg;
			}
			else
			{
			    //Return prev leg back to the init position
				model->LegsPos[model->PrevSelectedLeg].x = HexConfig::DefaultLegsPosX[model->PrevSelectedLeg];
				model->LegsPos[model->PrevSelectedLeg].y = HexConfig::DefaultLegsPosY[model->PrevSelectedLeg];
				model->LegsPos[model->PrevSelectedLeg].z = HexConfig::DefaultLegsPosZ[model->PrevSelectedLeg];
			}
		}
		else if (!model->SingleLegHold)
		{
			model->LegsPos[model->SelectedLeg].x = HexConfig::DefaultLegsPosX[model->SelectedLeg] + model->SingleLegPos.x;
			model->LegsPos[model->SelectedLeg].y = HexConfig::DefaultLegsPosY[model->SelectedLeg] + model->SingleLegPos.y;
			model->LegsPos[model->SelectedLeg].z = HexConfig::DefaultLegsPosZ[model->SelectedLeg] + model->SingleLegPos.z;
		}
	}
	else
	{
	    //All legs to init position
		if (!AllDown)
		{
			for (int LegIndex = 0; LegIndex < model->LegsCount; LegIndex++)
			{
				model->LegsPos[LegIndex].x = HexConfig::DefaultLegsPosX[LegIndex];
				model->LegsPos[LegIndex].y = HexConfig::DefaultLegsPosY[LegIndex];
				model->LegsPos[LegIndex].z = HexConfig::DefaultLegsPosZ[LegIndex];
			}
		}
		if (model->PrevSelectedLeg != 0xFF)
			model->PrevSelectedLeg = 0xFF;
	}
}

void Controller::Gait(int GaitCurrentLegNr)
{
    // Try to reduce the number of time we look at GaitLegnr and Gaitstep
	int LegStep = model->GaitStep - model->gaitCur->GaitLegNr[GaitCurrentLegNr];

	//Leg middle up position OK
	//Gait in motion	                                                                                  
	// For Lifted pos = 1, 3, 5
	if ((model->TravelRequest && ((model->gaitCur->NrLiftedPos & 1) > 0) && LegStep == 0) || 
	    (!model->TravelRequest && LegStep == 0 && ((fabs(model->GaitPos[GaitCurrentLegNr].x) > 2) || (fabs(model->GaitPos[GaitCurrentLegNr].z) > 2) || (fabs(model->GaitRotY[GaitCurrentLegNr]) > 2))))
	{ //Up
		model->GaitPos[GaitCurrentLegNr].x = 0;
		model->GaitPos[GaitCurrentLegNr].y = -model->LegLiftHeight;
		model->GaitPos[GaitCurrentLegNr].z = 0;
		model->GaitRotY[GaitCurrentLegNr] = 0;
	}
	//Optional Half heigth Rear (2, 3, 5 lifted positions)
	else if (((model->gaitCur->NrLiftedPos == 2 && LegStep == 0) || (model->gaitCur->NrLiftedPos >= 3 && (LegStep == -1 || LegStep == (model->gaitCur->StepsInGait - 1)))) && model->TravelRequest)
	{
		model->GaitPos[GaitCurrentLegNr].x = -model->TravelLength.x / model->gaitCur->LiftDivFactor;
		model->GaitPos[GaitCurrentLegNr].y = -3 * model->LegLiftHeight / (3 + model->gaitCur->HalfLiftHeight);     //Easier to shift between div factor: /1 (3/3), /2 (3/6) and 3/4
		model->GaitPos[GaitCurrentLegNr].z = -model->TravelLength.z / model->gaitCur->LiftDivFactor;
		model->GaitRotY[GaitCurrentLegNr] = -model->TravelLength.y / model->gaitCur->LiftDivFactor;
	}
	// _A_	  
	// Optional Half heigth front (2, 3, 5 lifted positions)
	else if ((model->gaitCur->NrLiftedPos >= 2) && (LegStep == 1 || LegStep == -(model->gaitCur->StepsInGait - 1)) && model->TravelRequest)
	{
		model->GaitPos[GaitCurrentLegNr].x = model->TravelLength.x / model->gaitCur->LiftDivFactor;
		model->GaitPos[GaitCurrentLegNr].y = -3 * model->LegLiftHeight / (3 + model->gaitCur->HalfLiftHeight); // Easier to shift between div factor: /1 (3/3), /2 (3/6) and 3/4
		model->GaitPos[GaitCurrentLegNr].z = model->TravelLength.z / model->gaitCur->LiftDivFactor;
		model->GaitRotY[GaitCurrentLegNr] = model->TravelLength.y / model->gaitCur->LiftDivFactor;
	}

	//Optional Half heigth Rear 5 LiftedPos (5 lifted positions)
	else if (((model->gaitCur->NrLiftedPos == 5 && (LegStep == -2))) && model->TravelRequest)
	{
		model->GaitPos[GaitCurrentLegNr].x = -model->TravelLength.x / 2;
		model->GaitPos[GaitCurrentLegNr].y = -model->LegLiftHeight / 2;
		model->GaitPos[GaitCurrentLegNr].z = -model->TravelLength.z / 2;
		model->GaitRotY[GaitCurrentLegNr] = -model->TravelLength.y / 2;
	}

	//Optional Half heigth Front 5 LiftedPos (5 lifted positions)
	else if ((model->gaitCur->NrLiftedPos == 5) && (LegStep == 2 || LegStep == -(model->gaitCur->StepsInGait - 2)) && model->TravelRequest)
	{
		model->GaitPos[GaitCurrentLegNr].x = model->TravelLength.x / 2;
		model->GaitPos[GaitCurrentLegNr].y = -model->LegLiftHeight / 2;
		model->GaitPos[GaitCurrentLegNr].z = model->TravelLength.z / 2;
		model->GaitRotY[GaitCurrentLegNr] = model->TravelLength.y / 2;
	}
	//_B_
	//Leg front down position //bug here?  From _A_ to _B_ there should only be one gaitstep, not 2!
	//For example, where is the case of LegStep==0+2 executed when NRLiftedPos=3?
	else if ((LegStep == model->gaitCur->FrontDownPos || LegStep == -(model->gaitCur->StepsInGait - model->gaitCur->FrontDownPos)) && model->GaitPos[GaitCurrentLegNr].y < 0)
	{
		model->GaitPos[GaitCurrentLegNr].x = model->TravelLength.x / 2;
		model->GaitPos[GaitCurrentLegNr].z = model->TravelLength.z / 2;
		model->GaitRotY[GaitCurrentLegNr] = model->TravelLength.y / 2;
		model->GaitPos[GaitCurrentLegNr].y = 0;
	}
	//Move body forward      
	else
	{
		model->GaitPos[GaitCurrentLegNr].x = model->GaitPos[GaitCurrentLegNr].x - (model->TravelLength.x / model->gaitCur->TLDivFactor);
		model->GaitPos[GaitCurrentLegNr].y = 0;
		model->GaitPos[GaitCurrentLegNr].z = model->GaitPos[GaitCurrentLegNr].z - (model->TravelLength.z / model->gaitCur->TLDivFactor);
		model->GaitRotY[GaitCurrentLegNr] = model->GaitRotY[GaitCurrentLegNr] - (model->TravelLength.y / model->gaitCur->TLDivFactor);
	}
}

void Controller::GateSequence()
{
	//Check if the Gait is in motion - If not if we are going to start a motion try to align our Gaitstep to start with a good foot
    // for the direction we are about to go...

	model->TravelRequest = (fabs(model->TravelLength.x) > HexConfig::TravelDeadZone)
	       || (fabs(model->TravelLength.z) > HexConfig::TravelDeadZone)
	       || (fabs(model->TravelLength.y) > HexConfig::TravelDeadZone) || model->Walking || (model->ForceGaitStepCnt != 0);
	if (!model->TravelRequest)
	{
	    //Clear values under the cTravelDeadZone
		model->TravelLength.x = 0;
		model->TravelLength.z = 0;
		model->TravelLength.y = 0;
		//Gait NOT in motion, return to home position
	}

	//Calculate Gait sequence for all legs
	for (int LegIndex = 0; LegIndex < HexConfig::LegsCount; LegIndex++)
	{ 
		Gait(LegIndex);
	}

	//Advance to the next step
	model->GaitStep++;
	if (model->GaitStep > model->gaitCur->StepsInGait)
		model->GaitStep = 1;

	// If we have a force count decrement it now... 
	if (model->ForceGaitStepCnt > 0)
		model->ForceGaitStepCnt--;
}

void Controller::BalCalcOneLeg(double posX, double posZ, double posY, int BalLegNr)
{
    //Calculating totals from center of the body to the feet
	double CPR_Z = HexConfig::OffsetZ[BalLegNr] + posZ;
	double CPR_X = HexConfig::OffsetX[BalLegNr] + posX;
	double CPR_Y = 15 + posY;        // using the value 150 to lower the centerpoint of rotation 'g_InControlState.BodyPos.y +

	model->TotalTrans.y += posY;
	model->TotalTrans.z += CPR_Z;
	model->TotalTrans.x += CPR_X;

	model->TotalBal.y += (atan2(CPR_Z, CPR_X) * 180) / M_PI;
	model->TotalBal.z += ((atan2(CPR_Y, CPR_X) * 180) / M_PI) - 90; //Rotate balance circle 90 deg
	model->TotalBal.x += ((atan2(CPR_Y, CPR_Z) * 180) / M_PI) - 90; //Rotate balance circle 90 deg
}
        
void Controller::Balance()
{
	const int BalanceDivFactor = HexConfig::LegsCount;
    // Reset values used for calculation of balance
	model->TotalTrans.x = model->TotalTrans.y = model->TotalTrans.z = 0;
	model->TotalBal.x = model->TotalBal.y = model->TotalBal.z = 0;

	// Balance calculations
	if (model->BalanceMode)
	{
	    // Balance Legs
		for (int LegIndex = 0; LegIndex < (HexConfig::LegsCount / 2); LegIndex++)
		{
			BalCalcOneLeg(-model->LegsPos[LegIndex].x + model->GaitPos[LegIndex].x,
				model->LegsPos[LegIndex].z + model->GaitPos[LegIndex].z,
				(model->LegsPos[LegIndex].y - HexConfig::DefaultLegsPosY[LegIndex]) + model->GaitPos[LegIndex].y,
				LegIndex);
		}

		for (int LegIndex = (HexConfig::LegsCount / 2); LegIndex < HexConfig::LegsCount; LegIndex++)
		{
			BalCalcOneLeg(model->LegsPos[LegIndex].x + model->GaitPos[LegIndex].x,
				model->LegsPos[LegIndex].z + model->GaitPos[LegIndex].z,
				(model->LegsPos[LegIndex].y - HexConfig::DefaultLegsPosY[LegIndex]) + model->GaitPos[LegIndex].y,
				LegIndex);
		}

		// BalanceBody
		model->TotalTrans.z = model->TotalTrans.z / BalanceDivFactor;
		model->TotalTrans.x = model->TotalTrans.x / BalanceDivFactor;
		model->TotalTrans.y = model->TotalTrans.y / BalanceDivFactor;

		if (model->TotalBal.y > 0)        //Rotate balance circle by +/- 180 deg
			model->TotalBal.y -= 180;
		else
			model->TotalBal.y += 100;

		if (model->TotalBal.z < -180)    //Compensate for extreme balance positions that causes overflow
			model->TotalBal.z += 360;

		if (model->TotalBal.x < -180)    //Compensate for extreme balance positions that causes overflow
			model->TotalBal.x += 360;

		//Balance rotation
		model->TotalBal.y = -model->TotalBal.y / BalanceDivFactor;
		model->TotalBal.x = -model->TotalBal.x / BalanceDivFactor;
		model->TotalBal.z = model->TotalBal.z / BalanceDivFactor;
	}
}

void Controller::SolveIKLegs()
{
	XYZ bodyFKPos;
	IKLegResult legIK;
	for (uint8_t leg = 0; leg < model->LegsCount / 2; leg++)
	{
		bodyFKPos = ik->BodyFK(leg,
			-model->LegsPos[leg].x + model->BodyPos.x + model->GaitPos[leg].x - model->TotalTrans.x,
			model->LegsPos[leg].z + model->BodyPos.z + model->GaitPos[leg].z - model->TotalTrans.z,
			model->LegsPos[leg].y + model->BodyPos.y + model->GaitPos[leg].y - model->TotalTrans.y,
			model->GaitRotY[leg],
			model->BodyRot.x,
			model->BodyRot.z,
			model->BodyRot.y,
			model->TotalBal.x,
			model->TotalBal.z,
			model->TotalBal.y);
		legIK = ik->LegIK(leg,
			model->LegsPos[leg].x - model->BodyPos.x + bodyFKPos.x - (model->GaitPos[leg].x - model->TotalTrans.x),
			model->LegsPos[leg].z + model->BodyPos.z - bodyFKPos.z + (model->GaitPos[leg].z - model->TotalTrans.z),
			model->LegsPos[leg].y + model->BodyPos.y - bodyFKPos.y + (model->GaitPos[leg].y - model->TotalTrans.y));
		if (legIK.Solution != IKSolutionResultType::Error)
		{
			model->LegsAngle[leg] = legIK.Result;
		}
	}
	for (uint8_t leg = (uint8_t)(model->LegsCount / 2); leg < model->LegsCount; leg++)
	{
		bodyFKPos = ik->BodyFK(leg,
			model->LegsPos[leg].x - model->BodyPos.x + model->GaitPos[leg].x - model->TotalTrans.x,
			model->LegsPos[leg].z + model->BodyPos.z + model->GaitPos[leg].z - model->TotalTrans.z,
			model->LegsPos[leg].y + model->BodyPos.y + model->GaitPos[leg].y - model->TotalTrans.y,
			model->GaitRotY[leg],
			model->BodyRot.x,
			model->BodyRot.z,
			model->BodyRot.y,
			model->TotalBal.x,
			model->TotalBal.z,
			model->TotalBal.y);
		legIK = ik->LegIK(leg,
			model->LegsPos[leg].x + model->BodyPos.x - bodyFKPos.x + (model->GaitPos[leg].x - model->TotalTrans.x),
			model->LegsPos[leg].z + model->BodyPos.z - bodyFKPos.z + (model->GaitPos[leg].z - model->TotalTrans.z),
			model->LegsPos[leg].y + model->BodyPos.y - bodyFKPos.y + (model->GaitPos[leg].y - model->TotalTrans.y));
		if (legIK.Solution != IKSolutionResultType::Error)
		{
			model->LegsAngle[leg] = legIK.Result;
		}
	}
}

void Controller::UpdateServos(CoxaFemurTibia* results, ushort moveTime)
{
	for (byte i = 0; i < HexConfig::LegsCount; i++)
	{
		// tfc-cft => /"*-*"
		ushort tibiaPos = (ushort)(1500 + ((results[i].Tibia * 10) + ServoOffset[i * 3]) * ServoInv[i * 3]);
		ushort femurPos = (ushort)(1500 + ((results[i].Femur * 10) + ServoOffset[i * 3 + 1]) * ServoInv[i * 3 + 1]);
		ushort coxaPos = (ushort)(1500 + ((results[i].Coxa * 10) + ServoOffset[i * 3 + 2]) * ServoInv[i * 3 + 2]);
		sd->Move(ServoMap[i * 3], tibiaPos, moveTime);
		sd->Move(ServoMap[i * 3 + 1], femurPos, moveTime);
		sd->Move(ServoMap[i * 3 + 2], coxaPos, moveTime);
	}
}
void Controller::CommitServos()
{
	sd->Commit();

	frame_data_t dbgFrame;
	dbgFrame.travelLength.x = model->TravelLength.x;
	dbgFrame.travelLength.y = model->TravelLength.y;
	dbgFrame.travelLength.z = model->TravelLength.z;
	dbgFrame.bodyPos.x = model->BodyPos.x;
	dbgFrame.bodyPos.y = model->BodyPos.y;
	dbgFrame.bodyPos.z = model->BodyPos.z;
	dbgFrame.bodyRot.x = model->BodyRot.x;
	dbgFrame.bodyRot.y = model->BodyRot.y;
	dbgFrame.bodyRot.z = model->BodyRot.z;
	dbgFrame.turnedOn = model->PowerOn;
	memcpy(dbgFrame.servos, sd->GetServos(), NUMBER_OF_SERVO * sizeof(uint32_t));
	debugSP->write16(FRAME_HEADER_ID, &dbgFrame, sizeof(frame_data_t));
}