#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "SBUSInputDriver.h"
#include "ServoDriver.h"
#include "Controller.h"

HardwareSerial Log(0);
HardwareSerial SerialInput(1);
HardwareSerial SerialOutput(2);
SBUSInputDriver inputDrv(SerialInput);
ServoDriver sd(&SerialOutput);
Controller controller(&inputDrv, &sd);

void setup() 
{
	Log.begin(115200);
	//SerialInput.begin(100000, SERIAL_8E2, 9, 10, true);
    SerialOutput.begin(115200);
    //inputDrv.Setup();
    sd.Init();
    sd.MoveAll(0);
    sd.Commit();

    //controller.Setup();
}

void loop() 
{
    for (int i=0;i<6;i++){
        sd.Move(i*3+0, 0, 1000); //t
        sd.Move(i*3+1,-800, 1000); //f
        sd.Move(i*3+2, 0, 1000); //c
    }
    sd.Commit();
    delay(1000);

    for (int i=0;i<6;i++){
        sd.Move(i*3+0, 0, 1000); //t
        sd.Move(i*3+1,-500, 5000); //f
        sd.Move(i*3+2, 0, 1000); //c
    }
    sd.Commit();
    delay(5000);
    //inputDrv.Debug(true);
    //controller.Loop();
}