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
#define TICK_DELAY 10

#define ID_DATA_SZ 0xA
#define ID_DATA    0xB

u16 servoData[SERVO_COUNT];
u16 servoNewData[SERVO_COUNT];
s16 servoNewDataSteps[SERVO_COUNT];
u8* rxData;
u32 rxDataSize = 0;
vu16 rxID = 0, rxIndex = 0, rxBytesToRead = 0;

extern void initServo();
extern void processSerial();
extern void dataWait(u16 id, u8* buffer, u16 bytesToRead);

int main()
{
	initServo();
	uartInit(57600);
	uartSendStr("\r\nsc32 V2.0\r\n");
	rxDataSize = 0;
	dataWait(ID_DATA_SZ, (u8*)&rxDataSize, 4);
	while (1)
	{
		for (int sid = 0; sid < SERVO_COUNT; ++sid)
		{
			if (servoNewDataSteps[sid] != 0)
			{
				if ((servoData[sid] + servoNewDataSteps[sid]) < servoNewData[sid])
				{
					servoData[sid] += servoNewDataSteps[sid];
				}
				else if ((servoData[sid] + servoNewDataSteps[sid]) > servoNewData[sid])
				{
					servoData[sid] += servoNewDataSteps[sid];
				}
				else
				{
					servoData[sid] = servoNewData[sid];
					servoNewDataSteps[sid] = 0;
				}
			}
		}
		processSerial();
		_delay_ms(TICK_DELAY);
	}
}

void free_servo()
{
	for (int i = 0;i < SERVO_COUNT;i++)
	{
		servoData[i] = 0;
	}
}

void initServo()
{
	free_servo();
	pwmInit(PWMB, &servoData[0]);
	pwmInit(PWMC, &servoData[4]);
	pwmInit(PWME, &servoData[8]);
	pwmInit(PWMA, &servoData[12]);
	pwmInit(PWMD, &servoData[16]);
}

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
		dataWait(ID_DATA, (u8*)servoNewData, rxDataSize);
	}
	else if (id == ID_DATA)
	{
		rxDataSize = 0;
		dataWait(ID_DATA_SZ, (u8*)&rxDataSize, 4);
		uartSendStr("#\n\r");
		
		for (int sid = 0; sid < SERVO_COUNT; ++sid)
		{
			if (servoData[sid] == 0 || 
				servoNewData[sid] == servoData[sid]) 
			{
				servoData[sid] = servoNewData[sid];
				servoNewDataSteps[sid] = 0;
			}
			else
			{
				u16 delta = 1000 / TICK_DELAY;
				s16 diff = servoNewData[sid] - servoData[sid];
				servoNewDataSteps[sid] = diff/delta;
			}
		}
	}
}

void processSerial()
{
	while (uart_rx_fifo_not_empty_flag)
	{
		u8 rx = uartGetByte();
		if (rxBytesToRead > 0)
		{
			rxData[rxIndex++] = rx;
			if (rxIndex == rxBytesToRead)
			{
				dataReady(rxID);
			}
		}
	}
	if (uart_rx_fifo_ovf_flag)
	{
		//todo:
		uart_rx_fifo_ovf_flag = 0;
	}
}