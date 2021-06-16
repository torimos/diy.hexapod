#pragma once
#include "Platform.h"
#include <stdint.h>
#define LEGS_COUNT 6

typedef enum
{
	Walk      = 0,
	Translate,
	Rotate,
	SingleLeg,
	GPPlayer
} ControlModeType;

typedef enum 
{
	Ripple12 = 0,
	Tripod8,
	TripleTripod12,
	TripleTripod16,
	Wave24,
	Tripod6
} GaitType;

typedef struct
{
	double x;
	double y;
} XY;

typedef struct
{
	double x;
	double y;
	double z;
} XYZ;

typedef struct
{
	double Coxa;
	double Femur;
	double Tibia;
} CoxaFemurTibia;

typedef struct
{
	short NomGaitSpeed;     //Nominal speed of the gait
	uint8_t StepsInGait;         //Number of steps in gait
	uint8_t NrLiftedPos;         //Number of positions that a single leg is lifted [1-3]
	uint8_t FrontDownPos;        //Where the leg should be put down to ground
	uint8_t LiftDivFactor;       //Normaly: 2, when NrLiftedPos=5: 4
	uint8_t TLDivFactor;         //Number of steps that a leg is on the floor while walking
	uint8_t HalfLiftHeight;      // How high to lift at halfway up.
	uint8_t GaitLegNr[6];       //Init position of the leg
} PhoenixGait;

class HexModel
{
public:
	HexModel(int legsCount);
	~HexModel();
	void Debug(int level);
public:
	int LegsCount;
	CoxaFemurTibia* LegsAngle;
	XYZ* LegsPos;

	XYZ TotalTrans; // Balanse Trans
	XYZ TotalBal; // Balanse

	XYZ BodyPos; // Body position
	XYZ BodyRot; // X -Pitch, Y-Rotation, Z-Roll
	double BodyYShift;
	double BodyYOffset;

	int LegInitIndex;
	double LegsXZLength;
	uint16_t SelectedLeg;
	uint16_t PrevSelectedLeg;
	XYZ SingleLegPos;
	bool SingleLegHold;

	ControlModeType ControlMode;
	ControlModeType PrevControlMode;

	uint16_t MoveTime;
	uint16_t Speed;
	uint16_t PrevMoveTime;
	bool PowerOn;
	bool PrevPowerOn;
	int InputTimeDelay;

	int GaitsCount;
	PhoenixGait* Gaits;
	PhoenixGait* gaitCur;
	int SelectedGaitType;
	
	uint8_t GaitStep;
	XYZ* GaitPos;
	double* GaitRotY;
	XYZ TravelLength;
	bool BalanceMode;
	uint8_t ForceGaitStepCnt;
	double LegLiftHeight;
	bool Walking;
	bool TravelRequest;
	bool DoubleHeightOn;
	bool DoubleTravelOn;
	bool WalkMethod;
	int ExtraCycle;

	uint8_t GPSeq;

	bool LiftUpWarning;

	bool DebugOutput;
};

