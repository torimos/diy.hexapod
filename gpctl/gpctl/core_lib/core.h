#ifndef __CORE_H
#define __CORE_H

#if defined (STM32F10X) | defined (STM32F10X_CL)
#include "stm32f10x.h"
#include <misc.h>
#include <stm32f10x.h>
#include <stm32f10x_gpio.h>
#include <stm32f10x_rcc.h>
#include <stm32f10x_tim.h>
#include <stm32f10x_usart.h>
#elif defined(STM32F4XX)
#include "stm32f4xx.h"
#endif

#include "delay.h"

#if defined (STM32F10X) | defined (STM32F10X_CL)
#define gpio_write(port, pins, value) if (value) port->BSRR = pins; else port->BRR = pins;
#elif defined(STM32F4XX)

void gpio_pin_af_config(GPIO_TypeDef* port, u16 pinSource, u8 gpioAF);
#define gpio_write(port, pins, value) if (value) port->BSRRL = pins; else port->BSRRH = pins;
#endif

#define gpio_read(port, pins) (((port->IDR & (pins)) != (uint32_t)Bit_RESET) ? (uint8_t)Bit_SET : (uint8_t)Bit_RESET)
#define gpio_toggle(port, pin) port->ODR ^= pin
#define gpio_toggle_x(port, pin, t) {port->ODR ^= pin;delay_ms(t);port->ODR ^= pin;}
void gpio_init(GPIO_TypeDef* port, u16 pin, GPIOMode_TypeDef mode, GPIOSpeed_TypeDef speed
#if defined (STM32F4XX)
		,GPIOOType_TypeDef otype, GPIOPuPd_TypeDef pupd
#endif
);
void spi_gpio_init(SPI_TypeDef *SPIx, u8 isMasterMode);
inline u8 spi_send(SPI_TypeDef *SPIx, u8 byte);


void uart_init(USART_TypeDef* usart);
void uart_send(USART_TypeDef* usart, u8 data);
void uart_send_buffer(USART_TypeDef* usart, const u8 *buffer, u32 size);

void LED_Config();
void Led(uint8_t led, uint8_t state);

#endif
