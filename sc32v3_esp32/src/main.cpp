#include "Platform.h"
#include "HexConfig.h"
#include "HexModel.h"
#include "IKSolver.h"
#include "Stopwatch.h"
#include "BLEInputDriver.h"
#include "ServoDriver.h"
#include "Controller.h"
#include <ble_dev.h>

ServoDriver sd(&STM32uart, &DEBUGuart);
BLEInputDriver inputDrv;
Controller controller(&inputDrv, &sd);

void bleDataCallback(std::string source, uint8_t id, uint8_t* data, size_t length) {
//   Log.print("[");
//   Log.print(source.c_str());
//   Log.print("|");
//   Log.print(id);
//   Log.print("] data[");
//   Log.print(length);
//   Log.print("]: ");
//   for (int i=0;i<length;i++)
//       Log.printf("%02x ", data[i]);
//   Log.println();
  if (inputDrv.incomingDataProcessed)
	{
		if (id == 0x1 && sizeof(joy_data_t) == length)
		{
			memcpy(&inputDrv.incomingData, data, sizeof(joy_data_t));
			inputDrv.incomingDataProcessed = false;
		}
	}
}

void setup() {
  Platform_Init();
  inputDrv.Setup();
  sd.Init();
  sd.Reset();
  controller.Setup();
  delay(200);
  Log.println("Setup is done.");
  ble_begin(bleDataCallback);

// StateLed.Flash(CRGB(0,0,4), 3, 250);
// sd.MoveAll(1000);
// sd.Commit();
// StateLed.Flash(CRGB(0,4,0), 1, 150);
// delay(1000);
// sd.MoveAll(2000);
// sd.Commit();
// StateLed.Flash(CRGB(4,0,0), 1, 150);
// delay(1000);

}
int offsets[3] = {-450, 0, 450};
int colors[3][3] = { {4, 0, 0}, {0, 4, 0}, {0, 0, 4} };
uint mode = 3;
void loop() {
  uint idx = mode % 4;
  if (idx < 3)
  {
    sd.MoveAll(1500 + offsets[idx]);
    sd.Commit();
    StateLed.Flash(CRGB(colors[idx][0],colors[idx][1],colors[idx][2]), 1, 150);
    delay(200);
  }

  if(!digitalRead(USR_PIN))
  {
    if (idx < 4)
    {
      mode++;
    }
    delay(1000);
  }
  else
  {
    ble_run();
    controller.Loop();
  }


// sd.MoveAll(1000);
// sd.Commit();
// StateLed.Flash(CRGB(0,4,0), 1, 150);
// delay(1000);
// sd.MoveAll(2000);
// sd.Commit();
// StateLed.Flash(CRGB(4,0,0), 1, 150);
// delay(1000);
}