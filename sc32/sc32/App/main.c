#include "core.h"
#include "timer.h"
#include "uart.h"
#include "crc32.h"

//D:T4[4..1]-A:T1[4..1]-E:T8[4..1]-C:T3[4..1]-B:T2[4..1]
#define PWMA 0
#define PWMB 1
#define PWMC 2
#define PWMD 3
#define PWME 4

#define SERVO_PWM_PERIOD 20000
#define SERVO_PWM_PERIOD_MS (SERVO_PWM_PERIOD / 1000)
#define SERVO_PWM_PRESCALER 1000000
#define SERVO_COUNT 20
#define SERIAL_DATA_FRAME_SIZE (SERVO_COUNT*4 + 4)
#define SERIAL_DATA_FRAME_TIMEOUT 50

u32 serialData[SERIAL_DATA_FRAME_SIZE];
u32 _frameOffset = 0;
u32 _frameTicks = 0;
u8* _frame = (u8*)serialData;

typedef struct
{
	uint16_t position;
	int16_t positionDelta;
	uint16_t positionNew;
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
	for (int sid = 0; sid < 4; ++sid)
	{
		servo_typedef* servo = &servos[id * 4 + sid];
		if (servo->positionDelta)
		{
			servo->position += servo->positionDelta;
			if (servo->positionDelta > 0)
			{
				if (servo->position >= servo->positionNew) {
					servo->position = servo->positionNew;
					servo->positionDelta = 0;
				}
			}
			else if (servo->positionDelta < 0)
			{
				if (servo->position <= servo->positionNew) {
					servo->position = servo->positionNew;
					servo->positionDelta = 0;
				}
			}
		}
		pwmData[sid] = servo->position;
	}
}

void processSerialData()
{
	for (int sid = 0; sid < SERVO_COUNT; ++sid)
	{
		uint16_t moveTime = (serialData[sid] >> 16) & 0xFFFF;
		uint16_t positionNew = (serialData[sid]) & 0xFFFF;
		serialData[sid] = 0;
		int ticks = moveTime / SERVO_PWM_PERIOD_MS;
		if (servos[sid].position != positionNew &&
			servos[sid].position > 0 &&
			positionNew > 0)
		{
			servos[sid].positionNew = positionNew;
			servos[sid].positionDelta = (servos[sid].positionNew - servos[sid].position) / ticks;
		}
		else
		{
			servos[sid].position = positionNew;
			servos[sid].positionNew = 0;
			servos[sid].positionDelta = 0;
		}
	}
}

int main()
{
	uartInit(115200);
	uartSendStr("\r\nsc32 V2.0\r\n");
	clockInit(1000);
	servosInit();
	while (1)
	{
		while (uart_rx_fifo_not_empty_flag)
		{
			if (_frameTicks == 0)
			{
				system_ticks = 0;
				_frameTicks = SERIAL_DATA_FRAME_TIMEOUT;
			}
			_frame[_frameOffset++] = uartGetByte();
			if (_frameOffset >= SERIAL_DATA_FRAME_SIZE)
			{
				u32 crcdiff = crc32(serialData, SERVO_COUNT) - serialData[SERVO_COUNT];
				if (crcdiff == 0)
				{
					processSerialData();
					uartSendStr("OK");
				}
				else
				{
					uartSendStr("ER");
				}
				_frameTicks = _frameOffset = 0;
			}
		}
		if (_frameTicks && system_ticks >= _frameTicks)
		{
			_frameTicks = _frameOffset = 0;
		}
	}
}