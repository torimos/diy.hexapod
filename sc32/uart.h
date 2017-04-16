#include "core.h"

#define FIFO_BUFFER_SIZE 128

volatile extern uint8_t uart_rx_fifo_not_empty_flag; // this flag is automatically set and cleared by the software buffer
volatile extern uint8_t uart_rx_fifo_full_flag;      // this flag is automatically set and cleared by the software buffer
volatile extern uint8_t uart_rx_fifo_ovf_flag;       // this flag is not automatically cleared by the software buffer
volatile extern uint8_t uart_tx_fifo_full_flag;      // this flag is automatically set and cleared by the software buffer
volatile extern uint8_t uart_tx_fifo_ovf_flag;       // this flag is not automatically cleared by the software buffer
volatile extern uint8_t uart_tx_fifo_not_empty_flag; // this flag is automatically set and cleared by the software buffer

void uart_init();
void uart_send_byte(uint8_t byte);
void uart_send_str(uc8 *str);
uint8_t uart_get_byte(void);
