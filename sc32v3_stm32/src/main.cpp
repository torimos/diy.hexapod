#include <Arduino.h>
#include "logger.h"
#include "sc.h"

void setup() {
  Serial.end();
	logger.begin(115200);
	logger.println("SC32 SW V3.0.1 HW V2.0");

  sc_init(&Serial5);
}

void loop() {
  sc_loop();

  // sc_write_all(1500);
  // delay(1000);
  // sc_write_all(1000);
  // delay(1000);
}
