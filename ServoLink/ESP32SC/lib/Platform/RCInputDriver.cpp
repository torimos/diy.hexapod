#include "RCInputDriver.h"
#include "HexConfig.h"
#include "HexModel.h"
#include <math.h>

RCInputDriver::RCInputDriver()
{
    rc_ch[5] = new RCChannel(36);
    rc_ch[4] = new RCChannel(39);
    rc_ch[3] = new RCChannel(34);
    rc_ch[2] = new RCChannel(35);
    rc_ch[1] = new RCChannel(32);
    rc_ch[0] = new RCChannel(33);
	state.Reset();
    prev_state.Reset();
}

bool RCInputDriver::IsTerminate()
{
    return failSafe;
}

bool RCInputDriver::ProcessInput(HexModel* model)
{
	if (!failSafe)
	{
    	captureState(&state);
	}
    else
	{
        return false;
	}
	if (prev_state.IsEmpty()) 
	{
		prev_state = copyState(&state);
	}
	if (model == NULL)
		return true;

	bool adjustLegsPosition = false;

	XY thumbLeft = { .x = state.LeftThumbX - 128.0, .y = (state.LeftThumbY) };
	XY thumbRight = { .x = state.RightThumbX - 128.0, .y = -(state.RightThumbY - 128.0) };

    if (state.pwrHasChanged && state.pwrIsOn)
    {
		if (model->PowerOn)
		{
			turnOff(model);
			model->PowerOn = false;
		}
		else
		{
			model->PowerOn = true;
			adjustLegsPosition = true;
		}
		delay(200);
    }
    else if (model->PowerOn)
	{

        if (thumbLeft.y > 0 && (state.LeftThumbY != prev_state.LeftThumbY)) // BodyYOffset: 0, HexConfig::BodyStandUpOffset
		{
			model->BodyYOffset = thumbLeft.y;

			if (model->BodyYOffset > HexConfig::MaxBodyHeight)
			{
				model->BodyYOffset = HexConfig::MaxBodyHeight;
			} 
			else if (model->BodyYOffset < 0)
			{
				model->BodyYOffset = 0;
			}
			
			// if (model->BodyYOffset > 0)
			// 	model->BodyYOffset = 0;
			// else
			// 	model->BodyYOffset = HexConfig::BodyStandUpOffset;

			adjustLegsPosition = true;
		}

{
        // if (hasPressed(GamepadButtonFlags::Btn5)) // ControlMode: Translate, Walk, SingleLeg
		// {
		// 	if (model->ControlMode != ControlModeType::Translate)
		// 	{
		// 		model->ControlMode = ControlModeType::Translate;
		// 	}
		// 	else if (model->SelectedLeg == 0xFF)
		// 	{
		// 		model->ControlMode = ControlModeType::Walk;
		// 	}
		// 	else
		// 	{
		// 		model->ControlMode = ControlModeType::SingleLeg;
		// 	}
		// 	delay(200);
		// }
		// else if (hasPressed(GamepadButtonFlags::Btn6)) // ControlMode: Rotate, Walk, SingleLeg
		// {
		// 	if (model->ControlMode != ControlModeType::Rotate)
		// 	{
		// 		model->ControlMode = ControlModeType::Rotate;
		// 	}
		// 	else if (model->SelectedLeg == 0xFF)
		// 	{
		// 		model->ControlMode = ControlModeType::Walk;
		// 	}
		// 	else
		// 	{
		// 		model->ControlMode = ControlModeType::SingleLeg;
		// 	}
		// 	delay(200);
		// }
		// else if (hasPressed(GamepadButtonFlags::Btn3)) // Circle. ControlMode/SelectedLeg: SingleLeg/0x00, Walk,0xFF
		// {
		// 	if ((fabs(model->TravelLength.x) < HexConfig::TravelDeadZone)
		// 	  || (fabs(model->TravelLength.z) < HexConfig::TravelDeadZone)
		// 	  || (fabs(model->TravelLength.y) < HexConfig::TravelDeadZone))
		// 	{
		// 		if (model->ControlMode != ControlModeType::SingleLeg)
		// 		{
		// 			model->ControlMode = ControlModeType::SingleLeg;
		// 			if (model->SelectedLeg == 0xFF)  //Select leg if none is selected
		// 			{
		// 				model->SelectedLeg = 0; //Startleg
		// 			}
		// 		}
		// 		else
		// 		{
		// 			model->ControlMode = ControlModeType::Walk;
		// 			model->SelectedLeg = 0xFF;
		// 		}
		// 	}
		// }
		// else if (hasPressed(GamepadButtonFlags::Btn2)) // Cross. ControlMode: GPPlayer, Walk
		// {
		// 	if (model->ControlMode != ControlModeType::GPPlayer)
		// 	{
		// 		model->ControlMode = ControlModeType::GPPlayer;
		// 		model->GPSeq = 0;
		// 	}
		// 	else
		// 		model->ControlMode = ControlModeType::Walk;
		// }
		// else if (hasPressed(GamepadButtonFlags::Btn1)) // Square. BalanceMode: On/Off
		// {
		// 	model->BalanceMode = !model->BalanceMode;
		// }
		//else if (hasPressed(GamepadButtonFlags::Btn4)) // Triangle. BodyYOffset: 0, HexConfig::BodyStandUpOffset
		// {
		// 	if (model->BodyYOffset > 0)
		// 		model->BodyYOffset = 0;
		// 	else
		// 		model->BodyYOffset = HexConfig::BodyStandUpOffset;
		// 	adjustLegsPosition = true;
		// }
		// else if (isButtonPressed(state, GamepadButtonFlags::DPadUp, 50) == true) // Up. BodyYOffset: +5. max HexConfig::MaxBodyHeight
		// {
		// 	model->BodyYOffset += 5;
		// 	if (model->BodyYOffset > HexConfig::MaxBodyHeight)
		// 		model->BodyYOffset = HexConfig::MaxBodyHeight;
		// 	adjustLegsPosition = true;
		// }
		// else if (isButtonPressed(state, GamepadButtonFlags::DPadDown, 50) == true) // Down. BodyYOffset: -5. min 0
		// {
		// 	if (model->BodyYOffset > 5)
		// 		model->BodyYOffset -= 5;
		// 	else
		// 		model->BodyYOffset = 0;
		// 	adjustLegsPosition = true;
		// }
		// else if (isButtonPressed(state, GamepadButtonFlags::DPadRight, 50) == true) // Left. Speed: -50. min 0
		// {
		// 	if (model->Speed >= 50) model->Speed -= 50;
		// }
		// else if (isButtonPressed(state, GamepadButtonFlags::DPadLeft, 50) == true) // Right. Speed: +50. max 2000
		// {
		// 	if (model->Speed < 2000) model->Speed += 50;
		// }
}
        model->BodyYShift = 0;
		if (model->ControlMode == ControlModeType::Walk)
		{
			if (model->BodyPos.y > 0)
			{

			{
				// if (hasPressed(GamepadButtonFlags::Btn9) &&
				//     fabs(model->TravelLength.x) < HexConfig::TravelDeadZone //No movement
				//     && fabs(model->TravelLength.z) < HexConfig::TravelDeadZone
				//     && fabs(model->TravelLength.y * 2) < HexConfig::TravelDeadZone) //Select
				// {
				// 	model->SelectedGaitType++;
				// 	if ((int)model->SelectedGaitType >= model->GaitsCount)
				// 	{
				// 		model->SelectedGaitType = GaitType::Ripple12;
				// 	}
				// 	model->gaitCur = &model->Gaits[model->SelectedGaitType];
				// }
				// else if (hasPressedOnly(GamepadButtonFlags::LeftThumb)) //Double leg lift height
				// {
				// 	model->DoubleHeightOn = !model->DoubleHeightOn;
				// 	if (model->DoubleHeightOn)
				// 		model->LegLiftHeight = HexConfig::LegLiftDoubleHeight;
				// 	else
				// 		model->LegLiftHeight = HexConfig::LegLiftHeight;
				// }
				// else if (hasPressedOnly(GamepadButtonFlags::RightThumb)) //Double Travel Length
				// {
				// 	model->DoubleTravelOn = !model->DoubleTravelOn;
				// }
				// else if (hasPressed(GamepadButtonFlags::LeftThumb | GamepadButtonFlags::RightThumb)) // Switch between WalkMethod 1 && WalkMethod 2
				// {
				// 	model->WalkMethod = !model->WalkMethod;
				// }
			}
				//Walking
				if (model->WalkMethod)  //(Walk Methode) 
					model->TravelLength.z = -thumbRight.y; //Left Stick Up/Down  
				else
				{
					model->TravelLength.x = -thumbRight.x;
					model->TravelLength.z = -thumbRight.y;
				}

				if (!model->DoubleTravelOn)
				{  //(Double travel length)
					model->TravelLength.x = model->TravelLength.x / 1.75;
					model->TravelLength.z = model->TravelLength.z / 1.75;
				}

				model->TravelLength.y = -thumbLeft.x / 6; //Left Stick Left/Right 
				model->LiftUpWarning = false;
			}
			else
			{
				model->LiftUpWarning = true;
			}
		}
		else if (model->ControlMode == ControlModeType::Translate)
		{
			model->BodyPos.x = thumbRight.x / 2;
			model->BodyPos.z = thumbRight.y / 3;
			model->BodyRot.y = thumbLeft.x * 2;
			model->BodyYShift = -thumbLeft.y / 2;
		}
		else if (model->ControlMode == ControlModeType::Rotate)
		{
			model->BodyRot.x = thumbRight.y;
			model->BodyRot.y = thumbLeft.y * 2;
			model->BodyRot.z = -thumbRight.x;
			model->BodyYShift = thumbLeft.y / 2;
		}
		// else if (model->ControlMode == ControlModeType::SingleLeg)
		// {
		// 	if (hasPressed(GamepadButtonFlags::Btn9)) //Select
		// 	{
		// 		model->SelectedLeg++;
		// 		if (model->SelectedLeg >= HexConfig::LegsCount)
		// 		{
		// 			model->SelectedLeg = 0;
		// 		}
		// 	}

		// 	model->SingleLegHold = isButtonPressed(state, GamepadButtonFlags::Btn6) == true;
		// 	model->SingleLegPos.x = thumbRight.x; //Right Stick Right/Left
		// 	model->SingleLegPos.y = -thumbLeft.y; //Left Stick Up/Down
		// 	model->SingleLegPos.z = thumbRight.y; //Right Stick Up/Down
		// }
		model->InputTimeDelay = 128 - (int)fmax(fmax(fabs(thumbRight.x), fabs(thumbRight.y)), fmax(fabs(thumbLeft.x), fabs(thumbLeft.y)));
        if (model->InputTimeDelay < 0) model->InputTimeDelay = 128;
    }

    model->BodyPos.y = fmin(fmax(model->BodyYOffset + model->BodyYShift, 0), HexConfig::MaxBodyHeight);
	if (adjustLegsPosition)
	{
		adjustLegPositionsToBodyHeight(model);
	}
    prev_state = copyState(&state);
    return true;
}

void RCInputDriver::Debug(bool clear)
{   
    if (clear)
		Log.printf("\033[%d;%dH", 0, 0);
	else
        Log.println();
    Log.printf("%s ", failSafe ? "fail" : "safe");
    for(int i=0;i<RC_NUM_CHANNELS;i++)
    {
        Log.printf("%5d ", state.raw[i]);
    }
    Log.println();
    Log.printf("M: %s ", state.modeIsOn ? "on " : "off");
    Log.println();
    Log.printf("SC: %3d %3d %3d %3d ", state.saState, state.sbState, state.scState, state.sdState);
    Log.println();
    Log.printf("SP: %3d %3d %3d %3d ", prev_state.saState, prev_state.sbState, prev_state.scState, prev_state.sdState);
    Log.println();
    Log.printf("TC: %6.2f %6.2f | %6.2f %6.2f", state.LeftThumbX, state.LeftThumbY, state.RightThumbX, state.RightThumbY);
    Log.println();
    Log.printf("TP: %6.2f %6.2f | %6.2f %6.2f", prev_state.LeftThumbX, prev_state.LeftThumbY, prev_state.RightThumbX, prev_state.RightThumbY);
    Log.println();
}

void RCInputDriver::Setup()
{
    for (int i=0; i<RC_NUM_CHANNELS; i++) {
        pinMode(rc_ch[i]->pin(), INPUT_PULLDOWN);
        attachInterruptArg(rc_ch[i]->pin(), &calc_ch, rc_ch[i], CHANGE);
    }
    TaskHandle_t loopTask;
    xTaskCreate(input_loop, "RCInputLoopTask", 1024, this, 1, &loopTask);
}

void RCInputDriver::turnOff(HexModel* model)
{
	model->BodyPos.x = 0;
	model->BodyPos.y = 0;
	model->BodyPos.z = 0;
	model->BodyRot.x = 0;
	model->BodyRot.y = 0;
	model->BodyRot.z = 0;
	model->TravelLength.x = 0;
	model->TravelLength.z = 0;
	model->TravelLength.y = 0;
	model->BodyYOffset = 0;
	model->BodyYShift = 0;
	model->SelectedLeg = 255;
}

void RCInputDriver::calc_ch(void* arg) {  
    RCChannel* p = static_cast<RCChannel*>(arg);
    if (digitalRead(p->pin()) == HIGH) 
    {
        p->start();
    } 
    else 
    {
        p->end();
    }
}

void RCInputDriver::input_loop(void* arg)
{
    RCInputDriver* pThis = static_cast<RCInputDriver*>(arg);
    while(true)
    {
        noInterrupts();
        pThis->failSafe = false;
        unsigned long now = millis();
        for (int i=0;i<RC_NUM_CHANNELS;i++)
        {
            pThis->rc_ch[i]->value(true);
            pThis->failSafe |= (now - pThis->rc_ch[i]->timestamp()) > RC_KEEPALIVE_TIMEOUT;
        }
        interrupts();
        delay(1);
    }
}

void RCInputDriver::adjustLegPositionsToBodyHeight(HexModel* model)
{
	const double MIN_XZ_LEG_ADJUST = HexConfig::CoxaLength;
	const double MAX_XZ_LEG_ADJUST = HexConfig::CoxaLength + HexConfig::TibiaLength + HexConfig::FemurLength / 4;
	double hexIntXZ[] = { 111, 88, 86 };
	double hexMaxBodyY[] = { 20, 50, HexConfig::MaxBodyHeight };

	// Lets see which of our units we should use...
	// Note: We will also limit our body height here...
	model->BodyPos.y = fmin(model->BodyPos.y, HexConfig::MaxBodyHeight);
	double XZLength = hexIntXZ[2];
	int i;
	for (i = 0; i < 2; i++)
	{    
		// Don't need to look at last entry as we already init to assume this one...
		if (model->BodyPos.y <= hexMaxBodyY[i])
		{
			XZLength = hexIntXZ[i];
			break;
		}
	}
	if (i != model->LegInitIndex)
	{
		model->LegInitIndex = i;  // remember the current index...

		//now lets see what happens when we change the leg positions...
		if (XZLength > MAX_XZ_LEG_ADJUST)
			XZLength = MAX_XZ_LEG_ADJUST;
		if (XZLength < MIN_XZ_LEG_ADJUST)
			XZLength = MIN_XZ_LEG_ADJUST;

		// see if same length as when we came in
		if (XZLength == model->LegsXZLength)
			return;

		model->LegsXZLength = XZLength;
		int legIndex;
		for (legIndex = 0; legIndex < HexConfig::LegsCount; legIndex++)
		{
			model->LegsPos[legIndex].x = cos(M_PI * HexConfig::CoxaDefaultAngle[legIndex] / 180) * XZLength;  //Set start positions for each leg
			model->LegsPos[legIndex].z = -sin(M_PI * HexConfig::CoxaDefaultAngle[legIndex] / 180) * XZLength;
		}

		                // Make sure we cycle through one gait to have the legs all move into their new locations...
		model->ForceGaitStepCnt = model->Gaits[model->SelectedGaitType].StepsInGait;
	}
}

void RCInputDriver::captureState(RCInputState_t* s)
{
	RCInputState_t prev;
	prev = copyState(s);	
	noInterrupts();
	for (int i=0;i<RC_NUM_CHANNELS;i++)
	{
		s->raw[i] = ((int32_t)rc_ch[i]->value() - 1500);
	}
	interrupts();
	
    s->pwrIsOn = s->raw[RC_PWR] > 0;
    s->pwrHasChanged = prev.pwrIsOn != s->pwrIsOn;

    s->modeIsOn = s->raw[RC_MOD] > 0;
    s->modeHasChanged = prev.modeIsOn != s->modeIsOn;

    if (s->modeIsOn)
    {
        s->saState = 1 + s->raw[RC_SA] / 500;
        s->saHasChanged =  prev.saState != s->saState;
        s->sbState = 1 + s->raw[RC_SB] / 500;
        s->sbHasChanged =  prev.sbState != s->sbState;
        s->scState = 1 + s->raw[RC_SC] / 500;
        s->scHasChanged =  prev.scState != s->scState;
        s->sdState = 1 + s->raw[RC_SD] / 500;
        s->sdHasChanged =  prev.sdState != s->sdState;
    }
    else 
	{
		
        s->LeftThumbX = 128.0 + s->raw[RC_LX] / 4.0;
        s->LeftThumbY = 128.0 + s->raw[RC_LY] / 4.0;
        s->RightThumbX = 128.0 + s->raw[RC_RX] / 4.0;
        s->RightThumbY = 128.0 - s->raw[RC_RY] / 4.0;
    }
}

RCInputState_t RCInputDriver::copyState(RCInputState_t *s)
{
    RCInputState_t ns;
    ns.pwrIsOn = s->pwrIsOn;
    ns.pwrHasChanged = false;
    ns.modeIsOn = s->modeIsOn;
    ns.modeHasChanged = false;
    ns.saState = s->saState;
    ns.saHasChanged = false;
    ns.sbState = s->sbState;
    ns.sbHasChanged = false;
    ns.scState = s->scState;
    ns.scHasChanged = false;
    ns.sdState = s->sdState;
    ns.sdHasChanged = false;
	ns.LeftThumbX = s->LeftThumbX;
	ns.LeftThumbY = s->LeftThumbY;
	ns.RightThumbX = s->RightThumbX;
	ns.RightThumbY = s->RightThumbY;
	for (int i=0;i<RC_NUM_CHANNELS;i++)
	{
		ns.raw[i] = s->raw[i];
	}
    return ns;
}

void RCInputState_t::Reset() {
    pwrHasChanged = false;
    pwrIsOn = false;
    modeHasChanged = false;
    modeIsOn = false;
    saHasChanged = false;
    saState = 0;
    sbHasChanged = false;
    sbState = 0;
    scHasChanged = false;
    scState = 0;
    sdHasChanged = false;
    sdState = 0;
	LeftThumbX = LeftThumbY = 0;
	RightThumbX = RightThumbY = 0;
	for (int i=0;i<RC_NUM_CHANNELS;i++)
	{
		raw[i] = 0;
	}
}

bool RCInputState_t::IsEmpty() {
    return (saState == 0) && (sbState == 0) && (scState == 0) && (sdState == 0) &&
		(LeftThumbX == 0) && (LeftThumbY == 0) && (RightThumbX == 0) && (RightThumbY == 0);
}