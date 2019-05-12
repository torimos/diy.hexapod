#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "RCInputDriver.h"
#include "ServoDriver.h"
#include "Controller.h"

HardwareSerial Log(0);
HardwareSerial SerialOutput(2);
RCInputDriver _inputDrv;
ServoDriver _sd(&SerialOutput);
Controller controller(&_inputDrv, &_sd);

void setup() 
{
	Log.begin(115200);
    //SerialOutput.begin(115200);
    _inputDrv.Setup();
    //controller.Setup();
}

void loop() 
{
    _inputDrv.Debug();
    //controller.Loop();
}