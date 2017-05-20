#include "core.h"

void _rcc_enable_gpio(GPIO_TypeDef* port, FunctionalState state)
{
	u32 periph;
#if defined(STM32F10X) | defined(STM32F10X_CL)
	if (port == GPIOA) periph = RCC_APB2Periph_GPIOA;
	else if (port == GPIOB) periph = RCC_APB2Periph_GPIOB;
	else if (port == GPIOC) periph = RCC_APB2Periph_GPIOC;
	else if (port == GPIOD) periph = RCC_APB2Periph_GPIOD;
	else if (port == GPIOE) periph = RCC_APB2Periph_GPIOE;
	else if (port == GPIOF) periph = RCC_APB2Periph_GPIOF;
	else if (port == GPIOG) periph = RCC_APB2Periph_GPIOG;
	else return;
	RCC_APB2PeriphClockCmd(periph, state);
#elif defined(STM32F4XX)
	if (port == GPIOA) periph = RCC_AHB1Periph_GPIOA;
	else if (port == GPIOB) periph = RCC_AHB1Periph_GPIOB;
	else if (port == GPIOC) periph = RCC_AHB1Periph_GPIOC;
	else if (port == GPIOD) periph = RCC_AHB1Periph_GPIOD;
	else if (port == GPIOE) periph = RCC_AHB1Periph_GPIOE;
	else if (port == GPIOF) periph = RCC_AHB1Periph_GPIOF;
	else if (port == GPIOG) periph = RCC_AHB1Periph_GPIOG;
	else if (port == GPIOH) periph = RCC_AHB1Periph_GPIOH;
	else if (port == GPIOI) periph = RCC_AHB1Periph_GPIOI;
	else return;
	RCC_AHB1PeriphClockCmd(periph, state);
#endif
}

void gpio_init(GPIO_TypeDef* port, u16 pin, GPIOMode_TypeDef mode, GPIOSpeed_TypeDef speed
#if defined (STM32F4XX)
		,GPIOOType_TypeDef otype, GPIOPuPd_TypeDef pupd
#endif
)
{
	GPIO_InitTypeDef portInit;
	GPIO_StructInit(&portInit);

	portInit.GPIO_Pin = pin;
	portInit.GPIO_Mode = mode;
	portInit.GPIO_Speed = speed;
#if defined(STM32F4XX)
	portInit.GPIO_OType = otype;
	portInit.GPIO_PuPd = pupd;
#endif

	_rcc_enable_gpio(port, ENABLE);
	GPIO_Init(port, &portInit);
}

#if defined(STM32F4XX)

void gpio_pin_af_config(GPIO_TypeDef* port, u16 pinSource, u8 gpioAF)
{
	GPIO_PinAFConfig(port, pinSource, gpioAF);
}

#endif
