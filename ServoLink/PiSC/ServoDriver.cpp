#include "ServoDriver.h"
#include "CRC.h"

#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <wiringPi.h>
#include <wiringSerial.h>

static int CoxaOffset[] = { 20, -40, 0, -20, -40, -20 }; //LF LM LR RR RM RF
static int FemurOffset[] = { 30, 20, 50, -170, -120, -20 };//{   70,-100, -55,   0,  45, -40 }; //LF LM LR RR RM RF 
static int TibiaOffset[] = { 20, 60, -50, 30, 20, 20 };//{    0,  65, -30,  40,   0,   0 }; //LF LM LR RR RM RF
static uint8_t LegsMap[] = { 3, 4, 5, 2, 1, 0 };

ServoDriver::ServoDriver(const char* device)
{
	if ((fd = serialOpen(device, 9600)) < 0)
	{
		printf("Unable to open serial device: %s\n", strerror(errno));
	}
}

ServoDriver::~ServoDriver()
{
	serialClose(fd);
}

void ServoDriver::Move(int index, uint16_t position, uint16_t moveTime)
{
	_buffer[index] = (uint32_t)(moveTime << 16) | position;
}
        
void ServoDriver::MoveAll(uint16_t position, uint16_t moveTime)
{
	for (int i = 0; i < NUMBER_OF_SERVO; i++)
	{
		Move(i, position, moveTime);
	}
}

void ServoDriver::Commit()
{
	if (fd < 0) {
		printf("Serial device not initialized.\n");
		return;
	}
	long m = millis();
	_buffer[BUFFER_LENGTH - 1] = CRC::Calculate(_buffer, NUMBER_OF_SERVO, CRC::CRC_32_MPEG2());
	long t = millis() - m;
	write(fd, _buffer, sizeof(uint32_t)*BUFFER_LENGTH);
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
