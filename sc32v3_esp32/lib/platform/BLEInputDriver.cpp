#include "BLEInputDriver.h"
#include "HexConfig.h"

#include <stdio.h>
#include <stdint.h>
#include <unistd.h>
#include <string.h>
#include <errno.h>
#include <math.h>

#define BUTTON_FORK BIT(0)
#define BUTTON_CIRCLE BIT(1)
#define BUTTON_SQUARE BIT(2)
#define BUTTON_TRIANGLE BIT(3)
#define BUTTON_LEFT_TUP BIT(4)
#define BUTTON_RIGHT_TUP BIT(5)
#define BUTTON_SELECT BIT(6)
#define BUTTON_START BIT(7)
#define BUTTON_UP BIT(8)
#define BUTTON_LEFT BIT(9)
#define BUTTON_DOWN BIT(10)
#define BUTTON_RIGHT BIT(11)
#define BUTTON_JSW_LEFT BIT(12)
#define BUTTON_JSW_RIGHT BIT(13)
#define BUTTON_MENU BIT(14)
#define BUTTON_LEFT_TDOWN BIT(15)
#define BUTTON_RIGHT_TDOWN BIT(16)


BLEInputDriver::BLEInputDriver()
{
}

BLEInputDriver::~BLEInputDriver()
{
}

void BLEInputDriver::Setup()
{
	_terminate = false;
	memset(&state, 0, sizeof(joy_data_t));
	memset(&prev_state, 0, sizeof(joy_data_t));
}

void BLEInputDriver::Debug(bool clear)
{
	if (clear)
		Log.printf("\033[%d;%dH", 0, 0);
	else
		Log.printf("\n\r");
	Log.printf("Buttons: %08x\n\r", state.buttons);
	Log.printf("Left: %05d %05d\n\r", state.axis_data[0], state.axis_data[1]);
	Log.printf("Right: %05d %05d\n\r", state.axis_data[2], state.axis_data[3]);
}

bool BLEInputDriver::isButtonPressed(input_state_t state, uint16_t flag, int delayMilliseconds)
{
	bool f = (state.buttons & flag) == flag;
	if (f && delayMilliseconds > 0) delay(delayMilliseconds);
	return f;
}

bool BLEInputDriver::isButtonPressedOnly(input_state_t state, uint16_t flag, int delayMilliseconds)
{
	bool f = state.buttons == flag;
	if (f && delayMilliseconds > 0) delay(delayMilliseconds);
	return f;
}

bool BLEInputDriver::hasPressed(uint16_t button)
{
	return ((state.buttons & button) == button) && ((prev_state.buttons & button) == 0);
}

bool BLEInputDriver::hasPressedOnly(uint16_t button)
{
	return ((state.buttons & button) == button) && ((prev_state.buttons & button) != button);
}

bool BLEInputDriver::ProcessInput(HexModel* model)
{
	if (incomingDataProcessed)
	{
		return false;
	}
	else
	{
		state.buttons = incomingData.buttons;
		state.axis_data[0] = (int16_t)((incomingData.axis_data[0]-2048) / 12);
		state.axis_data[1] = -(int16_t)((incomingData.axis_data[1]-2048) / 12);
		state.axis_data[2] = (int16_t)((incomingData.axis_data[2]-2048) / 12);
		state.axis_data[3] = -(int16_t)((incomingData.axis_data[3]-2048) / 12);
		state.initialized = true;
		incomingDataProcessed = true;
	}

	if (!prev_state.initialized)
	{
		memcpy(&prev_state, &state, sizeof(input_state_t));
	}
		
	
	if (hasPressed(BUTTON_MENU))
		_terminate = true;
	
	if (model == NULL)
		return true;

	bool adjustLegsPosition = false;
	
	XY thumbLeft = { .x = state.axis_data[0], .y = (state.axis_data[1]) };
	XY thumbRight = { .x = state.axis_data[2], .y = -(state.axis_data[3]) };
	
	if (hasPressed(BUTTON_START))
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
		if (hasPressed(BUTTON_LEFT_TUP)) // ControlMode: Translate, Walk, SingleLeg
		{
			if (model->ControlMode != ControlModeType::Translate)
			{
				model->ControlMode = ControlModeType::Translate;
			}
			else if (model->SelectedLeg == 0xFF)
			{
				model->ControlMode = ControlModeType::Walk;
			}
			else
			{
				model->ControlMode = ControlModeType::SingleLeg;
			}
			delay(200);
		}
		else if (hasPressed(BUTTON_RIGHT_TUP)) // ControlMode: Rotate, Walk, SingleLeg
		{
			if (model->ControlMode != ControlModeType::Rotate)
			{
				model->ControlMode = ControlModeType::Rotate;
			}
			else if (model->SelectedLeg == 0xFF)
			{
				model->ControlMode = ControlModeType::Walk;
			}
			else
			{
				model->ControlMode = ControlModeType::SingleLeg;
			}
			delay(200);
		}
		else if (hasPressed(BUTTON_CIRCLE)) // Circle. ControlMode/SelectedLeg: SingleLeg/0x00, Walk,0xFF
		{
			if ((fabs(model->TravelLength.x) < HexConfig::TravelDeadZone)
			  || (fabs(model->TravelLength.z) < HexConfig::TravelDeadZone)
			  || (fabs(model->TravelLength.y) < HexConfig::TravelDeadZone))
			{
				if (model->ControlMode != ControlModeType::SingleLeg)
				{
					model->ControlMode = ControlModeType::SingleLeg;
					if (model->SelectedLeg == 0xFF)  //Select leg if none is selected
					{
						model->SelectedLeg = 0; //Startleg
					}
				}
				else
				{
					model->ControlMode = ControlModeType::Walk;
					model->SelectedLeg = 0xFF;
				}
			}
		}
		else if (hasPressed(BUTTON_FORK)) // Cross. ControlMode: GPPlayer, Walk
		{
			if (model->ControlMode != ControlModeType::GPPlayer)
			{
				model->ControlMode = ControlModeType::GPPlayer;
				model->GPSeq = 0;
			}
			else
				model->ControlMode = ControlModeType::Walk;
		}
		else if (hasPressed(BUTTON_SQUARE)) // Square. BalanceMode: On/Off
		{
			model->BalanceMode = !model->BalanceMode;
		}
		else if (hasPressed(BUTTON_TRIANGLE)) // Triangle. BodyYOffset: 0, HexConfig::BodyStandUpOffset
		{
			if (model->BodyYOffset > 0)
				model->BodyYOffset = 0;
			else
				model->BodyYOffset = HexConfig::BodyStandUpOffset;

			adjustLegsPosition = true;
		}
		else if (isButtonPressed(state, BUTTON_UP, 50) == true) // Up. BodyYOffset: +5. max HexConfig::MaxBodyHeight
		{
			model->BodyYOffset += 5;
			if (model->BodyYOffset > HexConfig::MaxBodyHeight)
				model->BodyYOffset = HexConfig::MaxBodyHeight;
			adjustLegsPosition = true;
		}
		else if (isButtonPressed(state, BUTTON_DOWN, 50) == true) // Down. BodyYOffset: -5. min 0
		{
			if (model->BodyYOffset > 5)
				model->BodyYOffset -= 5;
			else
				model->BodyYOffset = 0;
			adjustLegsPosition = true;
		}
		else if (isButtonPressed(state, BUTTON_RIGHT, 50) == true) // Left. Speed: -50. min 0
		{
			if (model->Speed >= 50) model->Speed -= 50;
		}
		else if (isButtonPressed(state, BUTTON_LEFT, 50) == true) // Right. Speed: +50. max 2000
		{
			if (model->Speed < 2000) model->Speed += 50;
		}

		model->BodyYShift = 0;
		if (model->ControlMode == ControlModeType::Walk)
		{
			if (model->BodyPos.y > 0)
			{
				if (hasPressed(BUTTON_SELECT) &&
				    fabs(model->TravelLength.x) < HexConfig::TravelDeadZone //No movement
				    && fabs(model->TravelLength.z) < HexConfig::TravelDeadZone
				    && fabs(model->TravelLength.y * 2) < HexConfig::TravelDeadZone) //Select
				{
					model->SelectedGaitType++;
					if ((int)model->SelectedGaitType >= model->GaitsCount)
					{
						model->SelectedGaitType = GaitType::Ripple12;
					}
					model->gaitCur = &model->Gaits[model->SelectedGaitType];
				}
				else if (hasPressedOnly(BUTTON_LEFT_TDOWN)) //Double leg lift height - LThumb
				{
					model->DoubleHeightOn = !model->DoubleHeightOn;
					if (model->DoubleHeightOn)
						model->LegLiftHeight = HexConfig::LegLiftDoubleHeight;
					else
						model->LegLiftHeight = HexConfig::LegLiftHeight;
				}
				else if (hasPressedOnly(BUTTON_RIGHT_TDOWN)) //Double Travel Length - RThumb
				{
					model->DoubleTravelOn = !model->DoubleTravelOn;
				}
				else if (hasPressed(BUTTON_LEFT_TDOWN) && hasPressed(BUTTON_LEFT_TDOWN)) // Switch between WalkMethod 1 && WalkMethod 2
				{
					model->WalkMethod = !model->WalkMethod;
				}

				                        //Walking
				if (model->WalkMethod)  //(Walk Methode) 
					model->TravelLength.z = -thumbRight.y; //Right Stick Up/Down  
				else
				{
					model->TravelLength.x = -thumbLeft.x;
					model->TravelLength.z = -thumbLeft.y;
				}

				if (!model->DoubleTravelOn)
				{  //(Double travel length)
					model->TravelLength.x = model->TravelLength.x / 1.75;
					model->TravelLength.z = model->TravelLength.z / 1.75;
				}

				model->TravelLength.y = -thumbRight.x / 6; //Right Stick Left/Right 
				model->LiftUpWarning = false;
			}
			else
			{
				model->LiftUpWarning = true;
			}
		}
		else if (model->ControlMode == ControlModeType::Translate)
		{
			model->BodyPos.x = thumbLeft.x / 2;
			model->BodyPos.z = thumbLeft.y / 3;
			model->BodyRot.y = thumbRight.x * 2;
			model->BodyYShift = -thumbRight.y / 2;
		}
		else if (model->ControlMode == ControlModeType::Rotate)
		{
			model->BodyRot.x = thumbLeft.y;
			model->BodyRot.y = thumbRight.y * 2;
			model->BodyRot.z = -thumbLeft.x;
			model->BodyYShift = thumbRight.y / 2;
		}
		else if (model->ControlMode == ControlModeType::SingleLeg)
		{
			if (hasPressed(BUTTON_SELECT)) //Select
			{
				model->SelectedLeg++;
				if (model->SelectedLeg >= HexConfig::LegsCount)
				{
					model->SelectedLeg = 0;
				}
			}

			model->SingleLegHold = isButtonPressed(state, BUTTON_RIGHT_TUP) == true;
			model->SingleLegPos.x = thumbLeft.x; //Left Stick Right/Left
			model->SingleLegPos.y = -thumbRight.y; //Right Stick Up/Down
			model->SingleLegPos.z = thumbLeft.y; //Left Stick Up/Down
		}
		model->InputTimeDelay = 128 - (int)fmax(fmax(fabs(thumbLeft.x), fabs(thumbLeft.y)), fmax(fabs(thumbRight.x), fabs(thumbRight.y)));
		if (model->InputTimeDelay <= 0) model->InputTimeDelay = 1;
	}

	model->BodyPos.y = fmin(fmax(model->BodyYOffset + model->BodyYShift, 0), HexConfig::MaxBodyHeight);
	if (adjustLegsPosition)
	{
		adjustLegPositionsToBodyHeight(model);
	}
	memcpy(&prev_state, &state, sizeof(input_state_t));
	return true;
}

void BLEInputDriver::turnOff(HexModel* model)
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

void BLEInputDriver::adjustLegPositionsToBodyHeight(HexModel* model)
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
