#include "core.h"

void _rcc_enable_usart(USART_TypeDef* usart, FunctionalState state)
{
#if defined(STM32F10X) | defined(STM32F10X_CL)
	if (usart == USART1) RCC_APB2PeriphClockCmd(RCC_APB2Periph_USART1, state);
	else if (usart == USART2) RCC_APB1PeriphClockCmd(RCC_APB1Periph_USART2, state);
	else if (usart == USART3) RCC_APB1PeriphClockCmd(RCC_APB1Periph_USART3, state);
	else if (usart == UART4) RCC_APB1PeriphClockCmd(RCC_APB1Periph_UART4, state);
	else if (usart == UART5) RCC_APB1PeriphClockCmd(RCC_APB1Periph_UART5, state);
#elif defined(STM32F4XX)
	if (usart == USART3) RCC_APB1PeriphClockCmd(RCC_APB1Periph_USART3, state);
#endif
}

void _uart_init_gpio(USART_TypeDef* usart)
{
	_rcc_enable_usart(usart, ENABLE);
#if defined(STM32F10X) | defined(STM32F10X_CL)
	if (usart == USART3)
	{
		gpio_init(GPIOB,  GPIO_Pin_10, GPIO_Mode_AF_PP, GPIO_Speed_50MHz);//TX
		gpio_init(GPIOB,  GPIO_Pin_11, GPIO_Mode_IN_FLOATING, GPIO_Speed_50MHz);//RX
	}
#elif defined(STM32F4XX)
	if (usart == USART3)
	{
		gpio_pin_af_config(GPIOC, GPIO_PinSource10, GPIO_AF_USART3); //USARTx_Tx
		gpio_pin_af_config(GPIOC, GPIO_PinSource11, GPIO_AF_USART3); //USARTx_Rx
		gpio_init(GPIOC, GPIO_Pin_10 | GPIO_Pin_11, GPIO_Mode_AF, GPIO_Speed_50MHz, GPIO_OType_PP, GPIO_PuPd_NOPULL);
	}
#endif
}

IRQn_Type _usart_irq_channel(USART_TypeDef* usart)
{
	if (usart==USART1)
		return USART1_IRQn;
	if (usart==USART2)
		return USART2_IRQn;
	if (usart==USART3)
		return USART3_IRQn;
	return 0;
}

void uart_init(USART_TypeDef* usart)
{
	_uart_init_gpio(usart);
	/* USARTx configured as follow:
		- BaudRate = 115200 baud
		- Word Length = 8 Bits
		- One Stop Bit
		- No parity
		- Hardware flow control disabled (RTS and CTS signals)
		- Receive and transmit enabled
	*/
	USART_InitTypeDef usartInit;

	usartInit.USART_BaudRate = 9600;
	usartInit.USART_WordLength = USART_WordLength_8b;
	usartInit.USART_StopBits = USART_StopBits_1;
	usartInit.USART_Parity = USART_Parity_No;
	usartInit.USART_HardwareFlowControl = USART_HardwareFlowControl_None;
	usartInit.USART_Mode = USART_Mode_Rx | USART_Mode_Tx;

	USART_Init(usart, &usartInit);

//	USART_ITConfig(usart, USART_IT_RXNE, ENABLE);
//	USART_ITConfig(usart, USART_IT_TC, ENABLE);
//
//	NVIC_InitTypeDef NVIC_InitStructure;
//	NVIC_InitStructure.NVIC_IRQChannel = _usart_irq_channel(usart);
//	NVIC_InitStructure.NVIC_IRQChannelPreemptionPriority = 0;
//	NVIC_InitStructure.NVIC_IRQChannelSubPriority = 0;
//	NVIC_InitStructure.NVIC_IRQChannelCmd = ENABLE;
//	NVIC_Init(&NVIC_InitStructure);

	USART_Cmd(usart, ENABLE);
}

void uart_send(USART_TypeDef* usart, u8 data)
{
	if (USART_GetFlagStatus(usart, USART_FLAG_TXE) != RESET) return;
	USART_SendData(usart, data);
	while (USART_GetFlagStatus(usart, USART_FLAG_TC) == RESET){}
}

void uart_send_buffer(USART_TypeDef* usart, const u8 *buffer, u32 size)
{
    while(size--)
    {
        USART_SendData(usart, *buffer++);
        while(USART_GetFlagStatus(usart, USART_FLAG_TC) == RESET){}
    }
}
