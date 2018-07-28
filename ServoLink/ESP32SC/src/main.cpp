#include "Platform.h"
#include <Ticker.h>
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "ServoDriver.h"
#include "Controller.h"
#include "BluetoothSerialEx.h"

HardwareSerial Log(0);
HardwareSerial SerialOutput(2);
BluetoothSerialEx SerialBT;
SerialInputDriver _inputDrv(&SerialBT);
ServoDriver _sd(&SerialOutput);
Controller controller(&_inputDrv, &_sd);
Ticker ticker;
uint8_t devAddr[6] = {00,0x18,0x96,0xB0,0x01,0x3D};

void tickHandler()
{
    if (!SerialBT.hasClient())
    {
        ESP.restart();
    }
}


void setup() {
	Log.begin(230400);//, SERIAL_8N1, 23, 22, false);

    SerialBT.begin(devAddr);
    SerialOutput.begin(115200);
    controller.Setup();
    ticker.attach_ms(5000, tickHandler);
}

void loop() {

    controller.Loop();
}