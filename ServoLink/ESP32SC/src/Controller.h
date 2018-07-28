#pragma once
#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "ServoDriver.h"
#include "SerialInputDriver.h"

class Controller
{
	SerialInputDriver* inputDrv;
	ServoDriver* sd;
	HexModel* model;
	IKSolver* ik;
	Stopwatch *sw;	
public:
	Controller(SerialInputDriver* a, ServoDriver* b);
	~Controller();
	void Setup();
	bool Loop();
	
private:
	void Debug();
	void GPPlayer();
	void SingleLegControl();
	void Gait(int GaitCurrentLegNr);
	void GateSequence();
	void BalCalcOneLeg(double posX, double posZ, double posY, int BalLegNr);
	void Balance();
	void SolveIKLegs();
};
