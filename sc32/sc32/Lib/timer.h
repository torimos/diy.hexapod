#ifndef __TIMER_H
#define __TIMER_H

#include "core.h"

void timerInit(uint8_t index, uint8_t id, uint16_t period, uint32_t prescaler);
void timerEnable(uint8_t index, uint8_t enable);
extern void timerHandler(uint8_t index, uint8_t id, uint16_t *pwmData);

#endif
