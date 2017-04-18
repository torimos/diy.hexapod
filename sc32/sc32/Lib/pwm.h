#ifndef __PWM_H
#define __PWM_H

#include "core.h"

#define PWM_RANGE 1800

void pwmInit(uint8_t index, uint16_t *data);
void pwmEnable(uint8_t index, uint8_t enable);

#endif
