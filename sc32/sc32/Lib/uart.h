#ifndef __UART_H
#define __UART_H

#include "core.h"

#define FIFO_BUFFER_SIZE 128
#define UART_TX_FIFO_ENABLED 0

volatile uint8_t uart_rx_fifo_not_empty_flag; // this flag is automatically set and cleared by the software buffer
volatile uint8_t uart_rx_fifo_full_flag;      // this flag is automatically set and cleared by the software buffer
volatile uint8_t uart_rx_fifo_ovf_flag;       // this flag is not automatically cleared by the software buffer

#if UART_TX_FIFO_ENABLED
volatile uint8_t uart_tx_fifo_full_flag;      // this flag is automatically set and cleared by the software buffer
volatile uint8_t uart_tx_fifo_ovf_flag;       // this flag is not automatically cleared by the software buffer
volatile uint8_t uart_tx_fifo_not_empty_flag; // this flag is automatically set and cleared by the software buffer
#endif


void uart_init(uint32_t baudRate);
void uart_send_byte(uint8_t byte);
void uart_send_str(const char *pFormat, ...);
uint8_t uart_get_byte(void);

#endif
