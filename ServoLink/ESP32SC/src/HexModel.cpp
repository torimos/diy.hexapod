#include "HexModel.h"
#include <stdio.h>
#include <string.h>

HexModel::HexModel(int legsCount)
{
	memset(this, 0, sizeof(HexModel));
	LegsCount = legsCount;
	LegsAngle = new CoxaFemurTibia[legsCount];
	LegsPos = new XYZ[legsCount];

	TotalTrans = {};
	TotalBal = {};
	BodyPos = {};
	BodyRot = {};
	GaitPos = new XYZ[legsCount];
	GaitRotY = new double[legsCount];
	TravelLength = {};
	SingleLegPos = {};
	
	PrevControlMode = ControlMode = ControlModeType::Walk;
	
	memset(LegsAngle, 0, sizeof(CoxaFemurTibia)*legsCount);
	memset(LegsPos, 0, sizeof(XYZ)*legsCount);
	memset(GaitPos, 0, sizeof(XYZ)*legsCount);
	memset(GaitRotY, 0, sizeof(double)*legsCount);
}


HexModel::~HexModel()
{
	delete LegsAngle;
	delete LegsPos;
	delete GaitPos;
	delete GaitRotY;
	delete Gaits;
}

void HexModel::Debug(int level)
{
	int i = 0;
	if (level > 0 && level <= 0xF)
	{
		if ((level & 4) == 4)
		{
			Log.printf("LegsAngle:\n\r#,  Coxa,  Femur,  Tibia\n\r");
			for (i = 0; i < LegsCount; i++)
				printf("%d %6.1f %6.1f %6.1f %s\n\r", i, LegsAngle[i].Coxa, LegsAngle[i].Femur, LegsAngle[i].Tibia, (i == SelectedLeg ? "<<<<<" : "          "));
			Log.printf("\n\r");
			Log.printf("LegsPos:\n\r#,   X,   Y,   Z\n\r");
			for (i = 0; i < LegsCount; i++)
				Log.printf("%d %6.1f %6.1f %6.1f %s\n\r", i, LegsPos[i].x, LegsPos[i].y, LegsPos[i].z, (i == SelectedLeg ? "<<<<<" : "          "));
			Log.printf("\n\r");
		}
		if ((level & 2) == 2)
		{
			Log.printf("Body:\n\r     X,    Y,    Z,    RotX,  RotY,  RotZ, YOffs\n\r%6.1f %6.1f %6.1f %6.1f %6.1f %6.1f %6.1f\n\r",
				BodyPos.x,
				BodyPos.y,
				BodyPos.z,
				BodyRot.x,
				BodyRot.y,
				BodyRot.z,
				BodyYOffset);
			Log.printf("\n\r");
			Log.printf("TravelLength:\n\r     X,    Y,    Z\n\r%6.1f %6.1f %6.1f\n\r", TravelLength.x, TravelLength.y, TravelLength.z);
			Log.printf("\n\r");
		}
		if ((level& 1) == 1)
		{
			Log.printf("Gate [%d]:\n\r#,   X,     Y,     Z,     RotY\n\r", SelectedGaitType);
			for (i = 0; i < LegsCount; i++)
				Log.printf("%d %6.1f %6.1f %6.1f %6.1f\n\r", i, GaitPos[i].x, GaitPos[i].y, GaitPos[i].z, GaitRotY[i]);
		}
		Log.printf("\n\r");
	}
	Log.printf("Body Y: %6.1f\n\r", BodyPos.y);
	Log.printf("TravelRequest:%2d Walking:%2d GaitStep:%2d ForceGaitStepCnt:%2d ExtraCycle:%2d\n\r", TravelRequest, WalkMethod, GaitStep, ForceGaitStepCnt, ExtraCycle);
	Log.printf("WalkMethod:%2d  DoubleHeightOn:%2d  DoubleTravelOn:%2d\n\r", WalkMethod, DoubleHeightOn, DoubleTravelOn);
	Log.printf("Speed: %5d\n\r", Speed);
	Log.printf("InputTimeDelay: %5d\n\r", InputTimeDelay);
	Log.printf("ControlMode: %5d\n\r", ControlMode);
	Log.printf("PowerOn: %5d\n\r", PowerOn);
	if (LiftUpWarning)
		Log.printf("!!!Lift hexapod UP first!!!");
}