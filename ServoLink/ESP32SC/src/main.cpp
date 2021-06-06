#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "SBUSInputDriver.h"
#include "ServoDriver.h"
#include "Controller.h"

HardwareSerial Log(0);
HardwareSerial servoSerial(1);
HardwareSerial inputSerial(2);
SBUSInputDriver inputDrv(inputSerial);
ServoDriver sd(&servoSerial);
Controller controller(&inputDrv, &sd);

static int ServoMap[] = { 16,17,18, 19,12,13, 14,15, 8,  3,2,1,  0,7,6, 5,4,11 }; //tfc   //RR RM RF LR LM LF
static int ServoInv[] = { 1,1,1, 1,1,1, 1,1,1, -1,-1,-1, -1,-1,-1, -1,-1,-1 };//hack - inverse for calibration only
static int ServoOffset[] = { 10,-170,-30, -20,-130,-40, 0,-20,0, 20,80,30, 70,220,-40, -40,90,20 };
static int ServoPos[] = { -700, 700, 0, -700, 700, 0, -700, 700, 0, -700, 700, 0, -700, 700, 0, -700, 700, 0 };
bool run = true;
int leg = 0, last_leg = 0;
int offsetMax = 1000;
int offsetStep = 10;
void UpdateServos(ServoDriver* sd, ushort moveTime)
{
    //Log.printf("[%d] (%d) (%d) (%d)\n\r", leg, ServoOffset[leg * 3], ServoOffset[leg * 3 + 1], ServoOffset[leg * 3 + 2]);
	for (byte i = 0; i < HexConfig::LegsCount; i++)
	{
		// tfc-cft => /"*-*"
        ushort tibiaPos = (ushort)(1500 + (ServoPos[i * 3] + ServoOffset[i * 3]) * ServoInv[i * 3]);
        ushort femurPos = (ushort)(1500 + (ServoPos[i * 3 + 1] + ServoOffset[i * 3 + 1]) * ServoInv[i * 3 + 1]);
        ushort coxaPos = (ushort)(1500 + (ServoPos[i * 3 + 2] + ServoOffset[i * 3 + 2]) * ServoInv[i * 3 + 2]);
		sd->Move(ServoMap[i * 3], tibiaPos, moveTime);
		sd->Move(ServoMap[i * 3 + 1], femurPos, moveTime);
		sd->Move(ServoMap[i * 3 + 2], coxaPos, moveTime);
	}
    sd->Commit();
}

void setup() 
{
	Log.begin(115200);
	inputSerial.begin(100000, SERIAL_8E2, 16, 17, true);
    servoSerial.begin(115200, SERIAL_8N1, 27, 14);
    inputDrv.Setup();
    sd.Init();
    sd.Reset();
    controller.Setup();
    delay(200);
    // sd.Move(18, 1600, 1000);
    // sd.Commit();
    // delay(1000);
    // sd.Move(18, 1400, 1000);
    // sd.Commit();
    // delay(1000);
    // sd.Move(18, 1500, 1000);
    // sd.Commit();
    //UpdateServos(&sd, 0);
}

void loop() 
{
    inputDrv.Debug(true);
    controller.Loop();
    /*int key = 0;
    if (Log.available() > 0)
    {
        Log.readBytes((uint8_t*)&key, Log.available());
        //Log.printf("0x%X\n\r", key);
    }

    if (key > 0)
    {
        switch (key)
        {
            case 0x445B1B://LEFT
                last_leg = leg;
                leg--;
                if (leg < 0) leg = 0;
                break;
            case 0x435B1B://RIGHT
                last_leg = leg;
                leg++;
                if (leg >= HexConfig::LegsCount) leg = HexConfig::LegsCount - 1;
                break;
        }
        if (last_leg >= 0)
        {
            ServoPos[last_leg * 3] = -700;
            ServoPos[last_leg * 3 + 1] = 700;
            ServoPos[last_leg * 3 + 2] = 0;
            ServoPos[leg * 3] = 0;
            ServoPos[leg * 3 + 1] = 0;
            ServoPos[leg * 3 + 2] = 0;
            last_leg = -1;
        }
        UpdateServos(&sd, 0);
    }*/
}