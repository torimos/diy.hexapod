#include "sc.h"
#include "logger.h"
#include "timers.h"
#include "crc32.h"

#define NUMBER_OF_SERVO 26
#define SERVO_PWM_PERIOD 20000
#define SERVO_PWM_PERIOD_MS (SERVO_PWM_PERIOD / 1000)

typedef struct
{
	uint16_t position; // Holds current servo position
	int16_t positionDelta;
	uint16_t positionNew;
} servo_typedef;

servo_typedef servos[NUMBER_OF_SERVO];
const int servos_map[NUMBER_OF_SERVO] = {3,2,1,0, 7,6,5,4, 11,10,9,8, 15,14,13,12, 19,18,17,16, 20,21,22,23,24,25};

HardwareSerial* _input;

void processServoData(uint32_t* data);

void sc_init(HardwareSerial* inputSerial) {
	sc_write_all(0);
	initServos(SERVO_PWM_PERIOD);
	_input = inputSerial;
	_input->begin(115200, SERIAL_8N1);//921600
}

uint8_t rx_buf[128];
uint8_t frame_buf[256];
int frame_buf_len = 0;
int frame_cnt = 0;

#pragma pack(push, 1)
typedef struct {
	uint32_t header;
	uint16_t len;
	uint32_t data[NUMBER_OF_SERVO];
	uint32_t crc;
} uart_frame_t;
#pragma pack(pop)

size_t uart_read(uint8_t *buffer, size_t size)
{
    size_t avail = _input->available();
    if (size < avail) {
        avail = size;
    }
    size_t count = 0;
    while(count < avail) {
        *buffer++ = _input->read();
        count++;
    }
    return count;
}

size_t frame_buf_read()
{
  size_t rx_len =  uart_read(rx_buf, sizeof(rx_buf));
  _input->flush();
  if (rx_len <= 0) 
    return rx_len;
  else
  {
    if ((frame_buf_len + rx_len) > sizeof(frame_buf))
    {
      // data overflow
      frame_buf_len = 0;
    }
    memcpy(frame_buf+frame_buf_len, rx_buf, rx_len);
    frame_buf_len += rx_len;
    return rx_len;
  }
  return 0;
}
char sbuf[256];
int parse_frame()
{
    if (frame_buf_len > 0)
    {
        int frame_start_offset = 0;
        bool header_found = false;
        while(frame_start_offset < (frame_buf_len-4))
        {
            uint32_t* header = (uint32_t*)&frame_buf[frame_start_offset];
            if (*header ==  0x5332412B)
            {
                header_found = true;
                break;
            }
            frame_start_offset++;
        }
        if (header_found)
        {	
            // align frame start with 0 start index in buffer
            memmove(frame_buf, &frame_buf[frame_start_offset], frame_buf_len-frame_start_offset);
            frame_buf_len = frame_buf_len-frame_start_offset;
            if ((sizeof(frame_buf) - frame_buf_len) >= 1)
                memset(frame_buf+frame_buf_len, 0x55, sizeof(frame_buf) - frame_buf_len);

            if (frame_buf_len >= sizeof(uart_frame_t))
            {
				uart_frame_t* frame = (uart_frame_t*)frame_buf;
                int16_t data_size = frame->len;
				if (frame->len == sizeof(frame->data))
				{
					uint32_t expected_crc32 = get_CRC32((uint8_t*)frame->data, data_size);
					bool crc_valid = expected_crc32 == frame->crc;
					if (crc_valid)
					{
						processServoData(frame->data);
						frame_buf_len = 0;
						logger.print("@");
						//logger.print(frame_cnt++, 16);
						for (int i=0;i<NUMBER_OF_SERVO;i++) {
							sprintf(sbuf,"%08X ", frame->data[i]);
							logger.print(sbuf);
						}
						logger.println();
						return 1;
					}
					// else {
					// 	logger.print("#CRC ERR. crc received ");
					// 	logger.print(frame->crc, 16);
					// 	logger.print(" but expected ");
					// 	logger.print(expected_crc32, 16);
					// 	logger.println();
						
					// 	logger.print("#");
					// 	for (int i=0;i<frame_buf_len;i++) {
					// 		logger.print(frame_buf[i], 16);
					// 		logger.print(' ');
					// 	}
					// 	logger.println();
					// }
				}
            }
        }
    }
    return 0;
}

void sc_loop() {
	parse_frame();
	frame_buf_read();
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

void processServoData(uint32_t* data)
{
	for (int sid = 0; sid < NUMBER_OF_SERVO; ++sid)
	{
		uint16_t moveTime = (data[sid] >> 16) & 0xFFFF;
		uint16_t positionNew = (data[sid]) & 0xFFFF;
		//data[sid] = 0;
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