#include "sc.h"
#include "logger.h"
#include "timers.h"
#include "SerialProtocol.h"

#define FRAME_TO_SC_HEADER_ID 0xFB2C
#define FRAME_DEBUG_HEADER_ID 0x412C

#define NUMBER_OF_SERVO 26
#define SERVO_PWM_PERIOD 20000
#define SERVO_PWM_PERIOD_MS (SERVO_PWM_PERIOD / 1000)

typedef struct
{
	uint16_t position; // Holds current servo position
	int16_t positionDelta;
	uint16_t positionNew;
} servo_typedef;

#pragma pack(push, 1)
typedef struct {
	uint32_t header;
	uint16_t len;
	uint32_t data[NUMBER_OF_SERVO];
	uint32_t crc;
} uart_frame_t;
#pragma pack(pop)

servo_typedef servos[NUMBER_OF_SERVO];
uint32_t servo_data[NUMBER_OF_SERVO];

SerialProtocol* _sp;
SerialProtocol* _debugSP;

void processServoData(uint32_t* data);

void sc_init(HardwareSerial* inputSerial) {
	sc_write_all(0);
	initServos(SERVO_PWM_PERIOD);
	_sp = new SerialProtocol(inputSerial);
	#if DEBUG_SERVO_DATA
	_debugSP = new SerialProtocol(&logger);
	#endif
}

void sc_loop() {
	if (_sp->read(FRAME_TO_SC_HEADER_ID, servo_data, sizeof(uint32_t)*NUMBER_OF_SERVO))
	{
		processServoData(servo_data);
		#if DEBUG_SERVO_DATA
		_debugSP->write(FRAME_DEBUG_HEADER_ID, servo_data, sizeof(uint32_t)*NUMBER_OF_SERVO);
		#endif
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
	for (uint8_t sid = 0; sid < NUMBER_OF_SERVO; sid++)
	{
		servos[sid].position = us;
		servos[sid].positionNew = 0;
		servos[sid].positionDelta = 0;
	}
}

uint16_t timer_getPWMValue(uint8_t sid)
{
	servo_typedef* servo = &servos[sid];
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
	return servo->position;
}

void processServoData(uint32_t* data)
{
	for (int sid = 0; sid < NUMBER_OF_SERVO; ++sid)
	{
		uint16_t moveTime = (data[sid] >> 16) & 0xFFFF;
		uint16_t positionNew = (data[sid]) & 0xFFFF;

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