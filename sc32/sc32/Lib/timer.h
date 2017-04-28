#ifndef __TIMER_H
#define __TIMER_H

#define TIMER12_CLOCK

#include "core.h"

void timerInit(uint8_t index, uint8_t id, uint16_t period, uint32_t prescaler);
void timerEnable(uint8_t index, uint8_t enable);
extern void timerHandler(uint8_t index, uint8_t id, uint16_t *pwmData);

#ifdef TIMER12_CLOCK
extern uint32_t system_ticks;
void clockInit(uint16_t period);
#endif

#endif
