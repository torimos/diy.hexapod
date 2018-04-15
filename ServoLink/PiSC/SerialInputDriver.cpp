#include "SerialInputDriver.h"
#include "HexConfig.h"
#include <stdio.h>
#include <stdint.h>

#include <unistd.h>
#include <string.h>
#include <errno.h>
#include <math.h>

#include <wiringPi.h>
#include <wiringSerial.h>

SerialInputDriver::SerialInputDriver(const char* device)
{
	if ((fd = serialOpen(device, 9600)) < 0)
	{
		printf("Unable to open serial device: %s\n", strerror(errno));
	}
	
	Terminate = false;
	memset(&state, 0, sizeof(GamePadState));
	memset(&prev_state, 0, sizeof(GamePadState));
}

SerialInputDriver::~SerialInputDriver()
{
	serialClose(fd);
}

uint64_t SerialInputDriver::readInput(int fd)
{
	uint64_t raw = 0;
	uint64_t chk = 0;
	int n = serialDataAvail(fd);
	if (n < 8) return 0;
	n = read(fd, &chk, 2);
	if (n != 2 || (chk & 0xFFF0) != 0xFD40) return 0;
	n = read(fd, &raw, 6);
	if (n != 6) return 0;
	return raw | (chk << 48);
}

GamePadState SerialInputDriver::parseInput(uint64_t rawState)
{
	GamePadState state;
	state.RawState = rawState;
	uint16_t chk = (uint16_t)((rawState >> 48) & 0xFFF0);
	if (chk != 0xFD40) rawState = 0xFD40000080808080;
	state.Buttons = (GamepadButtonFlags)((rawState >> 32) & 0x000FFFFF);
	state.LeftThumbX = (uint8_t)(rawState & 0xFF);
	state.LeftThumbY = (uint8_t)((rawState >> 8) & 0xFF);
	state.RightThumbX = (uint8_t)((rawState >> 16) & 0xFF);
	state.RightThumbY = (uint8_t)((rawState >> 24) & 0xFF);
	return state;
}

void SerialInputDriver::Debug()
{
	printf("\n");
	printf("GamePad - RAW: %08x%08x\n", (unsigned int)((state.RawState >> 32) & 0xFFFFFFFF), (unsigned int)(state.RawState & 0xFFFFFFFF));
	printf("Buttons: %04x\n", state.Buttons);
	printf("Left: %03d %03d\n", state.LeftThumbX, state.LeftThumbY);
	printf("Right: %03d %03d\n", state.RightThumbX, state.RightThumbY);
}

bool SerialInputDriver::isButtonPressed(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds)
{
	bool f = (state.Buttons & flag) == flag;
	if (f && delayMilliseconds > 0) delay(delayMilliseconds);
	return f;
}

bool SerialInputDriver::isButtonPressedOnly(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds)
{
	bool f = state.Buttons == flag;
	if (f && delayMilliseconds > 0) delay(delayMilliseconds);
	return f;
}

bool SerialInputDriver::hasPressed(GamepadButtonFlags button)
{
	return ((state.Buttons & button) == button) && ((prev_state.Buttons & button) == 0);
}

bool SerialInputDriver::hasPressedOnly(GamepadButtonFlags button)
{
	return ((state.Buttons & button) == button) && ((prev_state.Buttons & button) != button);
}

bool SerialInputDriver::ProcessInput(HexModel* model)
{
	uint64_t raw = readInput(fd);
	if (raw == 0) return false;
	state = parseInput(raw);
	if (prev_state.RawState == 0) 
		prev_state = state;
	
	if (hasPressed(GamepadButtonFlags::B5))
		Terminate = true;
	
	bool adjustLegsPosition = false;
	
	XY thumbLeft = { .x = state.LeftThumbX - 128.0, .y = -(state.LeftThumbY - 128.0) };
	XY thumbRight = { .x = state.RightThumbX - 128.0, .y = -(state.RightThumbY - 128.0) };
	
	if (hasPressed(GamepadButtonFlags::B10))
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
		if (hasPressed(GamepadButtonFlags::B5))
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
		else if (hasPressed(GamepadButtonFlags::B6))
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
		else if (hasPressed(GamepadButtonFlags::B3)) // Circle
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
		else if (hasPressed(GamepadButtonFlags::B2)) // Cross
		{
			if (model->ControlMode != ControlModeType::GPPlayer)
			{
				model->ControlMode = ControlModeType::GPPlayer;
				model->GPSeq = 0;
			}
			else
				model->ControlMode = ControlModeType::Walk;
		}
		else if (hasPressed(GamepadButtonFlags::B1)) // Square
		{
			model->BalanceMode = !model->BalanceMode;
		}
		else if (hasPressed(GamepadButtonFlags::B4)) // Triangle
		{
			if (model->BodyYOffset > 0)
				model->BodyYOffset = 0;
			else
				model->BodyYOffset = HexConfig::BodyStandUpOffset;

			adjustLegsPosition = true;
		}
		else if (isButtonPressed(state, GamepadButtonFlags::DPadUp, 50) == true)
		{
			model->BodyYOffset += 5;
			if (model->BodyYOffset > HexConfig::MaxBodyHeight)
				model->BodyYOffset = HexConfig::MaxBodyHeight;
			adjustLegsPosition = true;
		}
		else if (isButtonPressed(state, GamepadButtonFlags::DPadDown, 50) == true)
		{
			if (model->BodyYOffset > 5)
				model->BodyYOffset -= 10;
			else
				model->BodyYOffset = 0;
			adjustLegsPosition = true;
		}
		else if (isButtonPressed(state, GamepadButtonFlags::DPadRight, 50) == true)
		{
			if (model->Speed >= 50) model->Speed -= 50;
		}
		else if (isButtonPressed(state, GamepadButtonFlags::DPadLeft, 50) == true)
		{
			if (model->Speed < 2000) model->Speed += 50;
		}

		model->BodyYShift = 0;
		if (model->ControlMode == ControlModeType::Walk)
		{
			if (model->BodyPos.y > 0)
			{
				if (hasPressed(GamepadButtonFlags::B9) &&
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
				else if (hasPressedOnly(GamepadButtonFlags::LeftThumb)) //Double leg lift height
				{
					model->DoubleHeightOn = !model->DoubleHeightOn;
					if (model->DoubleHeightOn)
						model->LegLiftHeight = HexConfig::LegLiftDoubleHeight;
					else
						model->LegLiftHeight = HexConfig::LegLiftHeight;
				}
				else if (hasPressedOnly(GamepadButtonFlags::RightThumb)) //Double Travel Length
				{
					model->DoubleTravelOn = !model->DoubleTravelOn;
				}
				else if (hasPressed(GamepadButtonFlags::LeftThumb | GamepadButtonFlags::RightThumb)) // Switch between Walk method 1 && Walk method 2
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
			}
			else
			{
				printf("!!!Lift hexapod UP first!!!");
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
			if (hasPressed(GamepadButtonFlags::B9)) //Select
			{
				model->SelectedLeg++;
				if (model->SelectedLeg >= HexConfig::LegsCount)
				{
					model->SelectedLeg = 0;
				}
			}

			model->SingleLegHold = isButtonPressed(state, GamepadButtonFlags::B6) == true;
			model->SingleLegPos.x = thumbLeft.x; //Left Stick Right/Left
			model->SingleLegPos.y = -thumbRight.y; //Right Stick Up/Down
			model->SingleLegPos.z = thumbLeft.y; //Left Stick Up/Down
		}
		model->InputTimeDelay = 128 - (int)fmax(fmax(fabs(thumbLeft.x), fabs(thumbLeft.y)), fmax(fabs(thumbRight.x), fabs(thumbRight.y)));
	}

	model->BodyPos.y = fmin(fmax(model->BodyYOffset + model->BodyYShift, 0), HexConfig::MaxBodyHeight);
	if (adjustLegsPosition)
	{
		adjustLegPositionsToBodyHeight(model);
	}
	prev_state = state;
	return true;
}

void SerialInputDriver::turnOff(HexModel* model)
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

void SerialInputDriver::adjustLegPositionsToBodyHeight(HexModel* model)
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
