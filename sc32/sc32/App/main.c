#include "core.h"
#include "timer.h"
#include "uart.h"

//D:T4[4..1]-A:T1[4..1]-E:T8[4..1]-F:T12[2..1]-C:T3[4..1]-B:T2[4..1]
#define PWMA 0
#define PWMB 1
#define PWMC 2
#define PWMD 3
#define PWME 4
#define PWMF 5
#define SERVO_PWM_PERIOD 20000
#define SERVO_PWM_PRESCALER 1000000
#define SERVO_COUNT 20

#define ID_SERVO_DATA    0xB

u16 serialData[SERVO_COUNT];

typedef struct
{
	uint16_t position;
	uint16_t positionNew;
	int16_t positionDelta;
} servo_typedef;

servo_typedef servos[SERVO_COUNT];

void servoGrpupInit(uint8_t portId, uint8_t groupId)
{
	timerInit(portId, groupId, SERVO_PWM_PERIOD, SERVO_PWM_PRESCALER);
	for (u8 sid = 0; sid < 4, (groupId * 4 + sid) < SERVO_COUNT; sid++)
	{
		servo_typedef* servo = &servos[groupId * 4 + sid];
		servo->position = 0;
		servo->positionNew = 0;
		servo->positionDelta = 0;
	}
}

void servosInit()
{
	servoGrpupInit(PWMB, 0);
	servoGrpupInit(PWMC, 1);
	servoGrpupInit(PWME, 2);
	servoGrpupInit(PWMA, 3);
	servoGrpupInit(PWMD, 4);
}

void timerHandler(uint8_t timer, uint8_t id, uint16_t *pwmData)
{
	pwmData[0] = servos[id * 4].position;
	pwmData[1] = servos[id * 4 + 1].position;
	pwmData[2] = servos[id * 4 + 2].position;
	pwmData[3] = servos[id * 4 + 3].position;
}

int main()
{
	uartInit(57600);
	uartSendStr("\r\nsc32 V2.0\r\n");
	uartDataWait(ID_SERVO_DATA, (u8*)serialData, SERVO_COUNT * 2);
	
	while (1)
	{
		//for (int sid = 0; sid < SERVO_COUNT; ++sid)
		//{
			//if (servoNewDataSteps[sid] != 0)
			//{
				//if ((servoData[sid] + servoNewDataSteps[sid]) < servoNewData[sid])
				//{
					//servoData[sid] += servoNewDataSteps[sid];
				//}
				//else if ((servoData[sid] + servoNewDataSteps[sid]) > servoNewData[sid])
				//{
					//servoData[sid] += servoNewDataSteps[sid];
				//}
				//else
				//{
					//servoData[sid] = servoNewData[sid];
					//servoNewDataSteps[sid] = 0;
				//}
			//}
		//}
		//
		
		uartDataProcess();
		_delay_ms(10);
	}
}

void uartDataReady(u16 id)
{
	if (id == ID_SERVO_DATA)
	{
		uartDataWait(ID_SERVO_DATA, (u8*)serialData, SERVO_COUNT * 2);
		uartSendStr("#\n\r");
		
		for (int sid = 0; sid < SERVO_COUNT; ++sid)
		{
			servos[sid].position = serialData[sid];
			//if (servoData[sid] == 0 || 
				//servoNewData[sid] == servoData[sid]) 
			//{
				//servoData[sid] = servoNewData[sid];
				//servoNewDataSteps[sid] = 0;
			//}
			//else
			//{
				//u16 delta = 1000 / TICK_DELAY;
				//s16 diff = servoNewData[sid] - servoData[sid];
				//servoNewDataSteps[sid] = diff / delta;
			//}
		}
	}
}