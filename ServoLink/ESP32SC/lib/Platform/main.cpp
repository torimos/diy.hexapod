#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "RCInputDriver.h"
#include "ServoDriver.h"
#include "Controller.h"
#include <SBUS.h>
#include <HardwareSerial.h>

HardwareSerial Log(0);
//HardwareSerial SerialOutput(2);
// RCInputDriver inputDrv;
// ServoDriver sd(&SerialOutput);
// Controller controller(&inputDrv, &sd);

SBUS xmp(Serial2);

void setup() 
{
	Log.begin(115200);//, SERIAL_8N1, 23, 22, false);
    // SerialOutput.begin(115200);
    // inputDrv.Setup();
    // controller.Setup();

    Serial2.begin(100000, SERIAL_8E2, 16, 17, true);

    xmp.begin();
}

uint16_t channels[16];
bool failSafe;
bool lostFrame;

void loop() 
{
    if(xmp.read(&channels[0], &failSafe, &lostFrame)){
        //xmp.write(&channels[0]);
        if (!lostFrame)
        {
            Log.printf("%s ", failSafe ? "fail" : "safe");
            for(int i=0;i<16;i++)
            {
                Log.printf("%5d ", channels[i]);
            }
            Log.println();
        }
    }
    // //inputDrv.Debug();
    // controller.Loop();
}