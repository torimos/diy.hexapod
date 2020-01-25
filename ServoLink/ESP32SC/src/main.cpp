#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "SBUSInputDriver.h"
#include "ServoDriver.h"
#include "Controller.h"

HardwareSerial Log(0);
HardwareSerial SerialOutput(2);
SBUSInputDriver inputDrv;
ServoDriver sd(&SerialOutput);
Controller controller(&inputDrv, &sd);

void setup() 
{
	Log.begin(115200);
    SerialOutput.begin(115200);
    inputDrv.Setup();
    controller.Setup();
}

void loop() 
{
    //inputDrv.Debug(true);
    controller.Loop();
}