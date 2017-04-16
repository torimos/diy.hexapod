#include "core.h"
#include "pwm.h"
#include "uart.h"

//D:T4[4..1]-A:T1[4..1]-E:T8[4..1]-F:T12[2..1]-C:T3[4..1]-B:T2[4..1]
#define PWMA 0
#define PWMB 1
#define PWMC 2
#define PWMD 3
#define PWME 4
#define PWMF 5
#define SERVO_COUNT 20

#define ID_DATA_SZ 0xA
#define ID_DATA    0xB

vu8 uart_rx_fifo_not_empty_flag = 0;
vu8 uart_rx_fifo_full_flag      = 0;
vu8 uart_rx_fifo_ovf_flag       = 0;
vu8 uart_tx_fifo_not_empty_flag = 0;
vu8 uart_tx_fifo_full_flag      = 0;
vu8 uart_tx_fifo_ovf_flag       = 0;

u16 pd[SERVO_COUNT];
u32 pds=0;
u8* rxData;
vu16 rxID = 0, rxIndex = 0, rxBytesToRead = 0;

void reset_pwm();
void process_serial();

void dataWait(u16 id, u8* buffer, u16 bytesToRead)
{
	rxID = id;
	rxIndex = 0;
	rxBytesToRead = bytesToRead;
	rxData = buffer;
}

void dataReady(u16 id)
{
	if (id == ID_DATA_SZ)
	{
		dataWait(ID_DATA, (u8*)pd, pds);
	}
	else if (id == ID_DATA)
	{
		dataWait(ID_DATA_SZ, (u8*)&pds, 4);
		uart_send_str("OK!\n\r");
	}
}

int main(void)
{
	dataWait(ID_DATA_SZ, (u8*)&pds, 4);
	uart_init();
	reset_pwm();
	while(1)
	{
		process_serial();
		_delay_ms(10);
	}
}

void reset_pwm()
{
	int i;
	for(i=0;i<SERVO_COUNT;i++)
	{
		pd[i] = 1500;
	}

	pwm_init(PWMB, &pd[0]);
	pwm_init(PWMC, &pd[4]);
	pwm_init(PWME, &pd[8]);
	pwm_init(PWMA, &pd[12]);
	pwm_init(PWMD, &pd[16]);
}

void process_serial()
{
	while(uart_rx_fifo_not_empty_flag)
	{
		u8 rx = uart_get_byte();
		if (rxBytesToRead > 0)
		{
			rxData[rxIndex++] = rx;
			if (rxIndex == rxBytesToRead)
			{
				dataReady(rxID);
			}
		}
		else
		{
		}
	}
	if(uart_rx_fifo_ovf_flag)
	{
		//todo:
		uart_rx_fifo_ovf_flag = 0;
	}
}

