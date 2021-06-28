#include <Arduino.h>
#include "logger.h"
#include "sc.h"

#define ESP32_UART Serial5

void setup() {
  Serial.end();
	logger.begin(115200);
	ESP32_UART.begin(115200);
	logger.println("#id SC32 SW V3.0.1 HW V2.0");
  sc_init(&ESP32_UART);
}

void loop() {
  sc_loop();
}
