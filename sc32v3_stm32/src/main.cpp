#include <Arduino.h>
#include "logger.h"
#include "sc.h"

void setup() { 
  Serial.end();
  Serial1.end();
  Serial2.end();
  Serial3.end();
  Serial4.end();
  Serial5.end();
  
	logger.begin(115200);
	logger.println("SC32 SW V3.0.1 HW V2.0");
  sc_init();
}

void loop() {
  sc_loop();
}
