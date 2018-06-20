#include "ServoDriver.h"
#include "CRC.h"

#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

static int CoxaOffset[] = { 20, -40, 0, -20, -40, -20 }; //LF LM LR RR RM RF
static int FemurOffset[] = { 30, 20, 50, -170, -120, -20 };//{   70,-100, -55,   0,  45, -40 }; //LF LM LR RR RM RF 
static int TibiaOffset[] = { 20, 60, -50, 30, 20, 20 };//{    0,  65, -30,  40,   0,   0 }; //LF LM LR RR RM RF
static uint8_t LegsMap[] = { 3, 4, 5, 2, 1, 0 };

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
    xTaskCreate(
		write_thread,       /* Function that implements the task. */
		"servoworker",          /* Text name for the task. */
		10240,      /* Stack size in words, not bytes. */
		( void * ) &xTaskParams,    /* Parameter passed into the task. */
		1,/* Priority at which the task is created. */
		&xHandle );      /* Used to pass out the created task's handle. */
	delay(500);
}

void ServoDriver::Move(int index, uint16_t position, uint16_t moveTime)
{
	_servo[index] = (uint32_t)(moveTime << 16) | position;
}
        
void ServoDriver::MoveAll(uint16_t position, uint16_t moveTime)
{
	for (int i = 0; i < NUMBER_OF_SERVO; i++)
	{
		Move(i, position, moveTime);
	}
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
		this->_crcBuffer[i] = swapOctetsUInt32(this->_servo[i]);
	}
	this->_servo[BUFFER_LENGTH - 1] = CRC::Calculate(this->_crcBuffer, sizeof(uint32_t)*NUMBER_OF_SERVO, CRC::CRC_32_MPEG2());
	memcpy(xTaskParams.buffer, this->_servo, sizeof(uint32_t)*BUFFER_LENGTH);
	xTaskParams.ready = 1;
}

void ServoDriver::Update(CoxaFemurTibia* results, uint16_t moveTime)
{
	for (int i = 0; i < 6; i++)
	{
		uint16_t coxaPos = (uint16_t)(1500 + (results[i].Coxa * 10) + CoxaOffset[LegsMap[i]]);
		uint16_t femurPos = (uint16_t)(1500 + (results[i].Femur * 10) + FemurOffset[LegsMap[i]]);
		uint16_t tibiaPos = (uint16_t)(1500 + (results[i].Tibia * 10) + TibiaOffset[LegsMap[i]]);
		Move(LegsMap[i] * 3, tibiaPos, moveTime);
		Move(LegsMap[i] * 3 + 1, femurPos, moveTime);
		Move(LegsMap[i] * 3 + 2, coxaPos, moveTime);
	}
}

void ServoDriver::Reset()
{    
	MoveAll(0);
	Commit();
}
