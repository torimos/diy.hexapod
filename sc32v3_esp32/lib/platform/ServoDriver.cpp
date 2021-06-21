#include "ServoDriver.h"
#include "crc32.h"

#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

TaskHandle_t xHandle = NULL;
#pragma pack(push, 1)
typedef struct {
	uint32_t header;
	uint16_t len;
	uint32_t data[NUMBER_OF_SERVO];
	uint32_t crc;
} uart_frame_t;
#pragma pack(pop)

typedef struct {
	uart_frame_t frame;
	uint8_t ready;
	Stream* stream;
	Stream* debugStream;
} task_params;
static task_params xTaskParams;

void write_thread( void * ptr )
{
	auto params = (task_params *)ptr;
	while (1)
	{
		if (params->ready)
		{
			params->stream->write((uint8_t*)&params->frame, sizeof(uart_frame_t));
			params->debugStream->write((uint8_t*)&xTaskParams.frame, sizeof(uart_frame_t));
			params->ready = 0;
		}
		vTaskDelay(5 / portTICK_PERIOD_MS);
	}
}

ServoDriver::ServoDriver(Stream* stream, Stream* debugStream)
{
	this->stream = stream;
	this->debugStream = debugStream;
}

ServoDriver::~ServoDriver()
{
	if (xHandle)
		vTaskDelete( xHandle );
}

void ServoDriver::Init()
{
	xTaskParams.stream = stream;
	xTaskParams.debugStream = debugStream;
	#if WRITE_TO_SC32_IN_BACKGROUND
    xTaskCreate(
		write_thread,       /* Function that implements the task. */
		"servoworker",          /* Text name for the task. */
		10240,      /* Stack size in words, not bytes. */
		( void * ) &xTaskParams,    /* Parameter passed into the task. */
		1,/* Priority at which the task is created. */
		&xHandle );      /* Used to pass out the created task's handle. */
	#endif
	delay(500);
}

void ServoDriver::Move(int index, uint16_t position, uint16_t moveTime)
{ 
	_servos[index] = (uint32_t)(moveTime << 16) | position;
}
        
void ServoDriver::MoveAll(uint16_t position, uint16_t moveTime)
{
	for (int i = 0; i < NUMBER_OF_SERVO; i++)
	{
		Move(i, position, moveTime);
	}
}

void ServoDriver::Reset()
{    
	MoveAll(0);
	Commit();
}

void ServoDriver::Commit()
{
	xTaskParams.frame.header = 0x5332412B; // +A2S
	xTaskParams.frame.len = sizeof(uint32_t)*NUMBER_OF_SERVO;
	memcpy(xTaskParams.frame.data, this->_servos, xTaskParams.frame.len);
	xTaskParams.frame.crc = get_CRC32((uint8_t*)xTaskParams.frame.data, xTaskParams.frame.len);
	xTaskParams.ready = 1;
	#if !WRITE_TO_SC32_IN_BACKGROUND
	this->stream->write((uint8_t*)&xTaskParams.frame, sizeof(uart_frame_t));
	this->debugStream->write((uint8_t*)&xTaskParams.frame, sizeof(uart_frame_t));
	#endif
}
