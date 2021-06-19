#include "ServoDriver.h"
#include "CRC.h"

#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

TaskHandle_t xHandle = NULL;

typedef struct {
	uint8_t buffer[sizeof(uint32_t)*BUFFER_LENGTH];
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
			params->stream->write(params->buffer, sizeof(uint32_t)*BUFFER_LENGTH);
			params->ready = 0;
		}
		vTaskDelay(5 / portTICK_PERIOD_MS);
	}
}

ServoDriver::ServoDriver(Stream* stream)
{
	_stream = stream;
}

ServoDriver::~ServoDriver()
{
	if (xHandle)
		vTaskDelete( xHandle );
}

void ServoDriver::Init()
{
	xTaskParams.stream = _stream;
    // xTaskCreate(
	// 	write_thread,       /* Function that implements the task. */
	// 	"servoworker",          /* Text name for the task. */
	// 	10240,      /* Stack size in words, not bytes. */
	// 	( void * ) &xTaskParams,    /* Parameter passed into the task. */
	// 	1,/* Priority at which the task is created. */
	// 	&xHandle );      /* Used to pass out the created task's handle. */
	delay(500);
}

static uint32_t swapOctetsUInt32(uint32_t toSwap)
{
	uint32_t tmp = 0;
	tmp = toSwap >> 24;
	tmp = tmp | ((toSwap & 0xff0000) >> 8);
	tmp = tmp | ((toSwap & 0xff00) << 8);
	tmp = tmp | ((toSwap & 0xff) << 24);
	return tmp;
}

void ServoDriver::Commit()
{
	for (int i = 0; i < NUMBER_OF_SERVO; i++)
	{
		this->_crcBuffer[i] = swapOctetsUInt32(this->_servos[i]);
	}
	this->_servos[BUFFER_LENGTH - 1] = CRC::Calculate(this->_crcBuffer, sizeof(uint32_t)*NUMBER_OF_SERVO, CRC::CRC_32_MPEG2());
	memcpy(xTaskParams.buffer, this->_servos, sizeof(uint32_t)*BUFFER_LENGTH);
	//xTaskParams.ready = 1;
	this->_stream->write(xTaskParams.buffer, sizeof(uint32_t)*BUFFER_LENGTH);

	Log.print("#Commit: ");
	for (int i = 0; i < NUMBER_OF_SERVO; i++)
		Log.printf("%02x", this->_servos[i]);
	Log.println();
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
