#ifndef __UART_H
#define __UART_H

#include "core.h"

#define FIFO_BUFFER_SIZE 128
#define UART_TX_FIFO_ENABLED 0

#define UART_ERROR_FIFO_OVF 1

volatile uint8_t uart_rx_fifo_not_empty_flag; // this flag is automatically set and cleared by the software buffer
volatile uint8_t uart_rx_fifo_full_flag;      // this flag is automatically set and cleared by the software buffer
volatile uint8_t uart_rx_fifo_ovf_flag;       // this flag is not automatically cleared by the software buffer

#if UART_TX_FIFO_ENABLED
volatile uint8_t uart_tx_fifo_full_flag;      // this flag is automatically set and cleared by the software buffer
volatile uint8_t uart_tx_fifo_ovf_flag;       // this flag is not automatically cleared by the software buffer
volatile uint8_t uart_tx_fifo_not_empty_flag; // this flag is automatically set and cleared by the software buffer
#endif


void uartInit(uint32_t baudRate);
void uartSendByte(uint8_t byte);
void uartSendStr(const char *pFormat, ...);
uint8_t uartGetByte(void);
void uartDataProcess();
void uartDataWait(u16 id, u8* buffer, u16 bytesToRead);
extern void uartDataReady(u16 id);
extern void uartError(int code);

#endif
