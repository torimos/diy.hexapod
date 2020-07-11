#include "Platform.h"
#include "ServoDriver.h"

ServoDriver sd(&STM32uart);

void setup() {

  Platform_Init();

  sd.Init();
  sd.Reset();

  StateLed.Flash(CRGB(0,0,4), 3, 250);

}

void loop() {
  sd.MoveAll(1000);
  sd.Commit();
  delay(1000);
  sd.MoveAll(2000);
  sd.Commit();
  delay(1000);
}