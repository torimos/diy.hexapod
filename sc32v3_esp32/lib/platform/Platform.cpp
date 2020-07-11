#include "Platform.h"

const int STM32_NRST_PIN = 26;
HardwareSerial Logger(0);
HardwareSerial STM32uart(1);

void ResetSTM32()
{
  pinMode(STM32_NRST_PIN, OUTPUT);
  digitalWrite(STM32_NRST_PIN, LOW);
  delay(1);
  digitalWrite(STM32_NRST_PIN, HIGH);
  delay(5);
  STM32uart.begin(115200, SERIAL_8N1, 27, 14);
}

void Platform_Init()
{
  Logger.begin(115200);	
  Logger.printf("\033c");
  ResetSTM32();
}