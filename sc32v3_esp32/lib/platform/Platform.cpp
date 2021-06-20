#include "Platform.h"

const int STM32_NRST_PIN = 26;
HardwareSerial Log(0);
HardwareSerial STM32uart(1);
HardwareSerial DEBUGuart(2);

void ResetSTM32()
{
  pinMode(STM32_NRST_PIN, OUTPUT);
  digitalWrite(STM32_NRST_PIN, LOW);
  delay(5);
  digitalWrite(STM32_NRST_PIN, HIGH);
  delay(5);
  STM32uart.begin(115200, SERIAL_8N1, 27, 14);
}

void Platform_Init()
{
  ResetSTM32();
  DEBUGuart.begin(115200);
  Log.begin(115200);	
  //Log.printf("\033c");
}