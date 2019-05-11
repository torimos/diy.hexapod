#pragma once
#include <stdint.h>

class HexConfig
{
public:
	static int LegsCount;
	static double CoxaMin;
	static double CoxaMax;
	static double CoxaLength;

	static double FemurMin;
	static double FemurMax;
	static double FemurLength;

	static double TibiaMin;
	static double TibiaMax;
	static double TibiaLength;

	static short OffsetX[]; //RR RM RF LF LM LR
	static short OffsetZ[]; //RR RM RF LF LM LR

	static double CoxaDefaultAngle[]; //RR RM RF LF LM LR

	static double DefaultLegsPosX[]; //RR RM RF LF LM LR
	static double DefaultLegsPosY[]; //RR RM RF LF LM LR
	static double DefaultLegsPosZ[]; //RR RM RF LF LM LR

	static bool CoxaAngleInv[]; //RR RM RF LF LM LR
	static bool FemurAngleInv[]; //RR RM RF LF LM LR
	static bool TibiaAngleInv[]; //RR RM RF LF LM LR LF LM LR

	static double MaxBodyHeight;
	static double BodyStandUpOffset;
	static double LegLiftHeight;
	static double LegLiftDoubleHeight;
	static double GPlimit;
	static double TravelDeadZone;

	static uint16_t WalkingDelay;
	static uint16_t BalancingDelay;
	static uint16_t SingleLegControlDelay;
	
	static int GaitsCount;
};
