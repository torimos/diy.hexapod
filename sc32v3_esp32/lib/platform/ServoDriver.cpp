#include "ServoDriver.h"

#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

#define FRAME_TO_SC_HEADER_ID 0xFB2C

TaskHandle_t xHandle = NULL;

typedef struct {
	uint32_t frame_data[NUMBER_OF_SERVO];
	uint16_t frame_data_size;
	SerialProtocol* sp;
} task_params;
static task_params xTaskParams;

void write_thread( void * ptr )
{
	auto params = (task_params *)ptr;
	while (1)
	{
		if (params->frame_data_size > 0)
		{
			params->sp->write(FRAME_TO_SC_HEADER_ID, &xTaskParams.frame_data, params->frame_data_size);
			params->frame_data_size = 0;
		}
		vTaskDelay(1);
	}
}

ServoDriver::ServoDriver(Stream* stream)
{
	sp = new SerialProtocol(stream);
}

ServoDriver::~ServoDriver()
{
	if (xHandle)
		vTaskDelete( xHandle );
}

void ServoDriver::Init()
{
	xTaskParams.sp = sp;
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
	uint16_t sz = sizeof(uint32_t)*NUMBER_OF_SERVO;
	#if !WRITE_TO_SC32_IN_BACKGROUND
	sp->write(FRAME_TO_SC_HEADER_ID, &this->_servos, sz);
	#else
	memcpy(xTaskParams.frame_data, this->_servos, sz);
	xTaskParams.frame_data_size = sz;
	#endif
}
