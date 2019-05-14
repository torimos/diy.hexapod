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
RCInputDriver inputDrv;
ServoDriver sd(&SerialOutput);
Controller controller(&inputDrv, &sd);

void setup() 
{
	Log.begin(115200);//, SERIAL_8N1, 23, 22, false);
    SerialOutput.begin(115200);
    inputDrv.Setup();
    controller.Setup();
}

void loop() 
{
    //inputDrv.Debug();
    controller.Loop();
}