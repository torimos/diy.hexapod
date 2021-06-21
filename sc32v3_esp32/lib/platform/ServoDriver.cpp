#include "ServoDriver.h"
#include "crc32.h"

#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

TaskHandle_t xHandle = NULL;

typedef struct {
	uart_frame_t frame;
	uint8_t ready;
	Stream* stream;
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
			params->ready = 0;
		}
		vTaskDelay(5 / portTICK_PERIOD_MS);
	}
}

ServoDriver::ServoDriver(Stream* stream)
{
	this->stream = stream;
}

ServoDriver::~ServoDriver()
{
	if (xHandle)
		vTaskDelete( xHandle );
}

void ServoDriver::Init()
{
	xTaskParams.stream = stream;
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
	uint16_t servoDataSize = sizeof(uint32_t)*NUMBER_OF_SERVO;
	uint32_t crc = get_CRC32((uint8_t*)this->_servos, servoDataSize);

	xTaskParams.frame.header = FRAME_HEADER_ID;
	xTaskParams.frame.len = servoDataSize;
	xTaskParams.frame.crc = crc;
	memcpy(xTaskParams.frame.data, this->_servos, servoDataSize);

	xTaskParams.ready = 1;
	#if !WRITE_TO_SC32_IN_BACKGROUND
	this->stream->write((uint8_t*)&xTaskParams.frame, sizeof(uart_frame_t));
	#endif
}
