#include "sc.h"
#include "logger.h"
#include "timers.h"
#include "crc32.h"

#define SERVO_COUNT 26
#define SERVO_PWM_PERIOD 20000
#define SERVO_PWM_PERIOD_MS (SERVO_PWM_PERIOD / 1000)
#define SERIAL_DATA_FRAME_SIZE (SERVO_COUNT*4 + 4)
#define SERIAL_DATA_FRAME_TIMEOUT 50

typedef struct
{
	uint16_t position; // Holds current servo position
	int16_t positionDelta;
	uint16_t positionNew;
} servo_typedef;

servo_typedef servos[SERVO_COUNT];
const int servos_map[SERVO_COUNT] = {3,2,1,0, 7,6,5,4, 11,10,9,8, 15,14,13,12, 19,18,17,16, 20,21,22,23,24,25};

uint32_t serialData[SERIAL_DATA_FRAME_SIZE];
uint32_t _frameOffset = 0;
uint32_t _frameTicksTimeOut = 0;
uint32_t _frameCount = 0;
uint32_t systemTicks = 0;
uint8_t* _frame = (uint8_t*)serialData;
HardwareSerial* _input;

void processSerialData();

void sc_init(HardwareSerial* inputSerial) {
	sc_write_all(0);
	initServos(SERVO_PWM_PERIOD);
	_input = inputSerial;
	_input->begin(115200);
}

void sc_loop() {
	while (_input->available()>0)
	{
		if (_frameTicksTimeOut == 0)
		{
			systemTicks = 0;
			_frameTicksTimeOut = SERIAL_DATA_FRAME_TIMEOUT;
		}
		_frame[_frameOffset++] = _input->read();
		if (_frameOffset >= SERIAL_DATA_FRAME_SIZE)
		{
			uint32_t crcdiff = crc32(serialData, SERVO_COUNT) - serialData[SERVO_COUNT];
			if (crcdiff == 0)
			{
				processSerialData();
				logger.print("@");
				logger.print(_frameCount++, 16);
				for (int i=0;i<SERIAL_DATA_FRAME_SIZE;i++)
					logger.print(serialData[i], 16);
				logger.println();
			}
			#if DEBUG_LVL >= 5
			else
			{
				logger.println(" ER");
				for (int i=0;i<SERIAL_DATA_FRAME_SIZE;i++)
				{
					logger.print(serialData[i], 16);
				}
				logger.println();
			}
			#endif
			_frameTicksTimeOut = _frameOffset = 0;
		}
	}
	if (_frameTicksTimeOut && systemTicks >= _frameTicksTimeOut)
	{
		_frameTicksTimeOut = _frameOffset = 0;
	}
}

void sc_write(int sid, int us)
{
	servos[sid].position = us;
	servos[sid].positionNew = 0;
	servos[sid].positionDelta = 0;
}

void sc_write_all(int us)
{
	for (uint8_t sid = 0; sid < SERVO_COUNT; sid++)
	{
		servos[sid].position = us;
		servos[sid].positionNew = 0;
		servos[sid].positionDelta = 0;
	}
}

void timerHandler(uint8_t id, uint16_t *pwmData, uint16_t pwmDataSize)
{
	for (int sid = 0; sid < pwmDataSize; sid++)
	{
		servo_typedef* servo = &servos[servos_map[id * 4 + sid]];
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
			positionNew > 0 &&
			moveTime > 0)
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