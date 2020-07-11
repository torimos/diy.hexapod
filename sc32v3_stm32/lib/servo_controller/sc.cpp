#include "sc.h"
#include "logger.h"
#include "timers.h"
#include "crc32.h"

#define SERVO_COUNT 20
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
const int servos_map[SERVO_COUNT] = {3,2,1,0, 7,6,5,4, 11,10,9,8, 15,14,13,12, 19,18,17,16};

uint32_t serialData[SERIAL_DATA_FRAME_SIZE];
uint32_t _frameOffset = 0;
uint32_t _frameTicks = 0;
uint8_t* _frame = (uint8_t*)serialData;

#define _input Serial5

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

void servoGrpupInit(uint8_t groupId)
{
	for (uint8_t sid = 0; sid < 4, (groupId * 4 + sid) < SERVO_COUNT; sid++)
	{
		servo_typedef* servo = &servos[groupId * 4 + sid];
		servo->position = 0;
		servo->positionNew = 0;
		servo->positionDelta = 0;
	}
}

void timerHandler(uint8_t id, uint16_t *pwmData)
{
	for (int sid = 0; sid < 4; ++sid)
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


void sc_init() {
	_input.begin(115200);
    servoGrpupInit(0);
	servoGrpupInit(1);
	servoGrpupInit(2);
	servoGrpupInit(3);
	servoGrpupInit(4);
	initServos(SERVO_PWM_PERIOD);
}

void sc_loop() {
  while (_input.available()>0)
  {
    if (_frameTicks == 0)
    {
      system_ticks = 0;
      _frameTicks = SERIAL_DATA_FRAME_TIMEOUT;
    }
    _frame[_frameOffset++] = _input.read();
    if (_frameOffset >= SERIAL_DATA_FRAME_SIZE)
    {
      uint32_t crcdiff = crc32(serialData, SERVO_COUNT) - serialData[SERVO_COUNT];
      logger.print(millis(),16);
      logger.print(" CRC: ");
      if (crcdiff == 0)
      {
        processSerialData();
        logger.println("OK");
      }
      else
      {
        logger.flush();
        logger.println("ER");
        for (int i=0;i<SERIAL_DATA_FRAME_SIZE;i++)
        {
            logger.print(serialData[i], 16);
        }
        logger.println();
      }
      _frameTicks = _frameOffset = 0;
    }
  }
  if (_frameTicks && system_ticks >= _frameTicks)
  {
    _frameTicks = _frameOffset = 0;
  }
}

void sc_write(int sid, int us)
{
	servos[sid].position = us;
	servos[sid].positionNew = 0;
	servos[sid].positionDelta = 0;
}