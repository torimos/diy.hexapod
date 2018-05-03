#pragma once
#include "HexModel.h"
#include <stdint.h>
#define NUMBER_OF_SERVO 20
#define BUFFER_LENGTH (NUMBER_OF_SERVO + 1)
class ServoDriver
{
	int fd;
	uint32_t _servo[BUFFER_LENGTH];
	uint32_t _crcBuffer[BUFFER_LENGTH];
public:
	ServoDriver(const char* device);
	~ServoDriver();
	
	void Update(CoxaFemurTibia* results, uint16_t moveTime);
	void Reset();
	void Commit();
public:
	void Move(int index, uint16_t position, uint16_t moveTime = 0);
	void MoveAll(uint16_t position, uint16_t moveTime = 0);
};

