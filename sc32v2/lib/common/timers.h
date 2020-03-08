#pragma once
#include <Arduino.h>

void initServos(int period);
void clockInit(uint16_t period);
extern uint32_t system_ticks;
