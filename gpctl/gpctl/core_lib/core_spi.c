#include "core.h"

void _rcc_enable_spi(SPI_TypeDef *SPIx, FunctionalState state)
{
#ifdef STM32F10X
	if (SPIx == SPI1)
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_SPI1, state);
	else if (SPIx == SPI2)
		RCC_APB1PeriphClockCmd(RCC_APB1Periph_SPI2, state);
#elif defined(STM32F4XX)
	if (SPIx == SPI1)
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_SPI1, state);
	else if (SPIx == SPI2)
		RCC_APB1PeriphClockCmd(RCC_APB1Periph_SPI2, state);
	else if (SPIx == SPI3)
		RCC_APB1PeriphClockCmd(RCC_APB1Periph_SPI3, state);
#endif
}

void spi_gpio_init(SPI_TypeDef *SPIx, u8 isMasterMode)
{
	_rcc_enable_spi(SPIx, ENABLE);
#ifdef STM32F10X
	GPIOSpeed_TypeDef gpio_speed = GPIO_Speed_50MHz;
	if (SPIx == SPI1)
	{
		gpio_init(GPIOA, GPIO_Pin_5 | GPIO_Pin_7, isMasterMode ? GPIO_Mode_AF_PP : GPIO_Mode_IN_FLOATING, gpio_speed);//SCK, MOSI
		gpio_init(GPIOA, GPIO_Pin_6, isMasterMode ? GPIO_Mode_IN_FLOATING : GPIO_Mode_AF_PP, gpio_speed);//MISO
		if (isMasterMode) gpio_init(GPIOA, GPIO_Pin_4, GPIO_Mode_Out_PP, gpio_speed);//CS
	}
	else if (SPIx == SPI2)
	{
		gpio_init(GPIOB, GPIO_Pin_13 | GPIO_Pin_15, isMasterMode ? GPIO_Mode_AF_PP : GPIO_Mode_IN_FLOATING, gpio_speed);//SCK, MOSI
		gpio_init(GPIOB, GPIO_Pin_14, isMasterMode ? GPIO_Mode_IN_FLOATING : GPIO_Mode_AF_PP, gpio_speed);//MISO
		if (isMasterMode) gpio_init(GPIOB, GPIO_Pin_12, GPIO_Mode_Out_PP, gpio_speed);//CS
	}
#elif defined(STM32F4XX)
	GPIOSpeed_TypeDef gpio_speed = GPIO_Speed_50MHz;
	if (SPIx == SPI1)
	{
		gpio_init(GPIOA, GPIO_Pin_7 | GPIO_Pin_6 | GPIO_Pin_5, isMasterMode ? GPIO_Mode_AF : GPIO_Mode_IN, gpio_speed, GPIO_OType_PP, GPIO_PuPd_NOPULL);//SCK, MOSI
		if (isMasterMode) gpio_init(GPIOA, GPIO_Pin_4, GPIO_Mode_OUT, gpio_speed, GPIO_OType_PP, GPIO_PuPd_NOPULL);//CSN

		//SPI1 A7_MOSI,A6_MISO,A5_SCK,A4_CSN
		GPIO_PinAFConfig(GPIOA, GPIO_PinSource7, GPIO_AF_SPI1);
		GPIO_PinAFConfig(GPIOA, GPIO_PinSource6, GPIO_AF_SPI1);
		GPIO_PinAFConfig(GPIOA, GPIO_PinSource5, GPIO_AF_SPI1);
		if (!isMasterMode) GPIO_PinAFConfig(GPIOA, GPIO_PinSource4, GPIO_AF_SPI1);
	}
#endif
}

inline u8 spi_send(SPI_TypeDef *SPIx, u8 byte)
{
	SPI_I2S_SendData(SPIx, byte);
	while(SPI_I2S_GetFlagStatus(SPIx, SPI_I2S_FLAG_TXE) == RESET);
	while(SPI_I2S_GetFlagStatus(SPIx, SPI_I2S_FLAG_RXNE) == RESET);
	return SPI_I2S_ReceiveData(SPIx);
}


