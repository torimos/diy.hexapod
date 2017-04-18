#include "uart.h"
#include <stdio.h>
#include <stdarg.h>

typedef struct
{
	uint8_t  data_buf[FIFO_BUFFER_SIZE]; // FIFO buffer
	uint16_t i_first;                    // index of oldest data byte in buffer
	uint16_t i_last;                     // index of newest data byte in buffer
	uint16_t num_bytes;                  // number of bytes currently in buffer
} sw_fifo_typedef;

sw_fifo_typedef rx_fifo = { {0}, 0, 0, 0 }; // declare a receive software buffer
#if UART_TX_FIFO_ENABLED
sw_fifo_typedef tx_fifo = { {0}, 0, 0, 0 }; // declare a transmit software buffer
#endif

void uartSendByte(uint8_t byte)
{
#if UART_TX_FIFO_ENABLED
	if(tx_fifo.num_bytes == FIFO_BUFFER_SIZE)
	{
		uart_tx_fifo_ovf_flag = 1;
		USART_ITConfig(UART4, USART_IT_TXE, DISABLE);
	}
	else if(tx_fifo.num_bytes < FIFO_BUFFER_SIZE)
	{
		tx_fifo.data_buf[tx_fifo.i_last] = byte;
		tx_fifo.i_last++;
		tx_fifo.num_bytes++;
	}
	if(tx_fifo.num_bytes == FIFO_BUFFER_SIZE)
	{
		uart_tx_fifo_full_flag = 1;
	}
	if(tx_fifo.i_last == FIFO_BUFFER_SIZE)
	{
		tx_fifo.i_last = 0;
	}

	if(tx_fifo.num_bytes > 0)
	{
		uart_tx_fifo_not_empty_flag = 1;
		USART_ITConfig(UART4, USART_IT_TXE, ENABLE);
	}
#else
	while(USART_GetFlagStatus(UART4, USART_FLAG_TXE) == RESET);
	UART4->DR = byte;
#endif
}
char _buffer[256];
void uartSendStr(const char *pFormat, ...)
{
	va_list ap;
	signed int result;
	char *pStr = _buffer;
	// Forward call to vsprintf
	va_start(ap, pFormat);
	result = vsprintf(pStr, pFormat, ap);
	va_end(ap);
	while (*pStr != 0)
	{
#if UART_TX_FIFO_ENABLED
		while(uart_tx_fifo_full_flag);
		uart_send_byte(*str++);
#else
		UART4->DR = *pStr++;
		while(USART_GetFlagStatus(UART4, USART_FLAG_TXE) == RESET);
#endif
	}
}

uint8_t uartGetByte(void)
{
	uint8_t byte = 0;
	if (rx_fifo.num_bytes == FIFO_BUFFER_SIZE)
	{
		uart_rx_fifo_full_flag = 0;
	}
	if (rx_fifo.num_bytes > 0)
	{
		byte = rx_fifo.data_buf[rx_fifo.i_first];
		rx_fifo.i_first++;
		rx_fifo.num_bytes--;
	}
	if (rx_fifo.num_bytes == 0)
	{
		uart_rx_fifo_not_empty_flag = 0;
	}
	if (rx_fifo.i_first == FIFO_BUFFER_SIZE)
	{
		rx_fifo.i_first = 0;
	}
	return byte;
}

void uartInit(uint32_t baudRate)
{
	RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOC, ENABLE);
	RCC_APB1PeriphClockCmd(RCC_APB1Periph_UART4, ENABLE);

	GPIO_InitTypeDef GPIO_InitStructure;
	GPIO_InitStructure.GPIO_Pin = GPIO_Pin_10;
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_AF_PP;
	GPIO_InitStructure.GPIO_Speed = GPIO_Speed_50MHz;
	GPIO_Init(GPIOC, &GPIO_InitStructure);
	GPIO_InitStructure.GPIO_Pin = GPIO_Pin_11;
	GPIO_InitStructure.GPIO_Mode = GPIO_Mode_IN_FLOATING;
	GPIO_Init(GPIOC, &GPIO_InitStructure);

	USART_InitTypeDef USART_InitStructure;
	USART_InitStructure.USART_BaudRate = baudRate;
	USART_InitStructure.USART_WordLength = USART_WordLength_8b;
	USART_InitStructure.USART_StopBits = USART_StopBits_1;
	USART_InitStructure.USART_Parity = USART_Parity_No;
	USART_InitStructure.USART_HardwareFlowControl = USART_HardwareFlowControl_None;
	USART_InitStructure.USART_Mode = USART_Mode_Rx | USART_Mode_Tx;
	USART_Init(UART4, &USART_InitStructure);

	NVIC_InitTypeDef NVIC_InitStructure;
	NVIC_InitStructure.NVIC_IRQChannel = UART4_IRQn;
	NVIC_InitStructure.NVIC_IRQChannelPreemptionPriority = 0;
	NVIC_InitStructure.NVIC_IRQChannelSubPriority = 0;
	NVIC_InitStructure.NVIC_IRQChannelCmd = ENABLE;
	NVIC_Init(&NVIC_InitStructure);

	USART_ITConfig(UART4, USART_IT_RXNE, ENABLE);

	USART_Cmd(UART4, ENABLE);
}

void UART4_IRQHandler(void)
{
#if UART_TX_FIFO_ENABLED
	if(USART_GetITStatus(UART4, USART_IT_TXE) != RESET)
	{
		if(tx_fifo.num_bytes == FIFO_BUFFER_SIZE)
		{
			uart_tx_fifo_full_flag = 0;
		}
		if(tx_fifo.num_bytes > 0)
		{
			UART4->DR = tx_fifo.data_buf[tx_fifo.i_first];
			tx_fifo.i_first++;
			tx_fifo.num_bytes--;
		}
		if(tx_fifo.i_first == FIFO_BUFFER_SIZE)
		{
			tx_fifo.i_first = 0;
		}
		if(tx_fifo.num_bytes == 0)
		{
			uart_tx_fifo_not_empty_flag = 0;
			USART_ITConfig(UART4, USART_IT_TXE, DISABLE);
		}
	}
#endif
	if(USART_GetITStatus(UART4, USART_IT_RXNE) != RESET)
	{
		if(rx_fifo.num_bytes == FIFO_BUFFER_SIZE)
		{
			uart_rx_fifo_ovf_flag = 1;
		}
		else if(rx_fifo.num_bytes < FIFO_BUFFER_SIZE)
		{
			rx_fifo.data_buf[rx_fifo.i_last] = UART4->DR;
			rx_fifo.i_last++;
			rx_fifo.num_bytes++;
		}

		if(rx_fifo.num_bytes == FIFO_BUFFER_SIZE)
		{
			uart_rx_fifo_full_flag = 1;
		}

		if(rx_fifo.i_last == FIFO_BUFFER_SIZE)
		{
			rx_fifo.i_last = 0;
		}

		uart_rx_fifo_not_empty_flag = 1;
	}
}
