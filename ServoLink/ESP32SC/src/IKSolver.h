#pragma once
#include "HexModel.h"
#include "HexConfig.h"
#include <stdint.h>

typedef enum 
{
	Solution,
	Warning,
	Error
} IKSolutionResultType;

typedef struct
{
	CoxaFemurTibia Result;
	IKSolutionResultType Solution;
} IKLegResult;

class IKSolver
{
public:
	IKSolver();
	~IKSolver();
	IKLegResult LegIK(uint8_t legNumber, double feetPosX, double feetPosZ, double feetPosY);
	XYZ BodyFK(uint8_t legNumber, double PosX, double PosZ, double PosY, double RotationY, double BodyRotX, double BodyRotZ, double BodyRotY, double TotalXBal, double TotalZBal, double TotalYBal);
};

