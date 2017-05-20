#include "core.h"

void LED_Config()
{
#if defined(STM32F4XX)
	gpio_init(GPIOC, GPIO_Pin_12 | GPIO_Pin_13, GPIO_Mode_OUT, GPIO_Speed_50MHz, GPIO_OType_PP, GPIO_PuPd_NOPULL);
#endif
}

void Led(uint8_t led, uint8_t state)
{
#if defined(STM32F4XX)
	uint16_t leds[] = {GPIO_Pin_12, GPIO_Pin_13};
	GPIO_WriteBit(GPIOC, leds[led%2], state);
	gpio_write(GPIOC, leds[led%2], state);
#endif
}
