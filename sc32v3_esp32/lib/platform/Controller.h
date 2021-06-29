#pragma once
#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "Counter.h"
#include "ServoDriver.h"
#include "InputDriver.h"
#include "SerialProtocol.h"

class Controller
{
	InputDriver* inputDrv;
	ServoDriver* sd;
	HexModel* model;
	IKSolver* ik;
	Stopwatch *sw;
	Counter *cps;
	SerialProtocol* debugSP;
	settings_t settings;
public:
	Controller(InputDriver* a, ServoDriver* b, SerialProtocol* debugSP);
	~Controller();
	void Setup();
	bool Loop();
	
private:
	void GPPlayer();
	void SingleLegControl();
	void Gait(int GaitCurrentLegNr);
	void GateSequence();
	void BalCalcOneLeg(double posX, double posZ, double posY, int BalLegNr);
	void Balance();
	void SolveIKLegs();
	void UpdateServos(CoxaFemurTibia* results, ushort moveTime);
	void CommitServos();
	void SyncSettings();
	void LoadSettings();
};

