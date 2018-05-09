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
	if ((level & 4) == 4)
	{
		printf("LegsAngle:\n#,  Coxa,  Femur,  Tibia\n");
		for (i = 0; i < LegsCount; i++)
			printf("%d %6.1f %6.1f %6.1f %s\n", i, LegsAngle[i].Coxa, LegsAngle[i].Femur, LegsAngle[i].Tibia, (i == SelectedLeg ? "<<<<<" : "          "));
		printf("\n");
		printf("LegsPos:\n#,   X,   Y,   Z\n");
		for (i = 0; i < LegsCount; i++)
			printf("%d %6.1f %6.1f %6.1f %s\n", i, LegsPos[i].x, LegsPos[i].y, LegsPos[i].z, (i == SelectedLeg ? "<<<<<" : "          "));
		printf("\n");
	}
	if ((level & 2) == 2)
	{
		printf("Body:\n     X,    Y,    Z,    RotX,  RotY,  RotZ, YOffs\n%6.1f %6.1f %6.1f %6.1f %6.1f %6.1f %6.1f\n",
			BodyPos.x,
			BodyPos.y,
			BodyPos.z,
			BodyRot.x,
			BodyRot.y,
			BodyRot.z,
			BodyYOffset);
		printf("\n");
		printf("TravelLength:\n     X,    Y,    Z\n%6.1f %6.1f %6.1f\n", TravelLength.x, TravelLength.y, TravelLength.z);
		printf("\n");
	}
	if ((level& 1) == 1)
	{
		printf("Gate [%d]:\n#,   X,     Y,     Z,     RotY\n", SelectedGaitType);
		for (i = 0; i < LegsCount; i++)
			printf("%d %6.1f %6.1f %6.1f %6.1f\n", i, GaitPos[i].x, GaitPos[i].y, GaitPos[i].z, GaitRotY[i]);
	}
	printf("\n");
	printf("TravelRequest:%2d Walking:%2d GaitStep:%2d ForceGaitStepCnt:%2d ExtraCycle:%2d\n", TravelRequest, WalkMethod, GaitStep, ForceGaitStepCnt, ExtraCycle);
	printf("WalkMethod:%2d  DoubleHeightOn:%2d  DoubleTravelOn:%2d\n", WalkMethod, DoubleHeightOn, DoubleTravelOn);
	printf("Speed: %5d\n", Speed);
	printf("InputTimeDelay: %5d\n", InputTimeDelay);
	printf("ControlMode: %5d\n", ControlMode);
	printf("PowerOn: %5d\n", PowerOn);
}