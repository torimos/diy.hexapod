#pragma once
#include "Platform.h"
//#include "HexModel.h"
#include "Stream.h"
#include <stdint.h>

#define NUMBER_OF_SERVO 26
class ServoDriver
{
	uint32_t _servos[NUMBER_OF_SERVO];
	Stream *_stream;
public:
	ServoDriver(Stream* stream);
	~ServoDriver();
	
	void Init();
	void Reset();
	void Move(int index, uint16_t position, uint16_t moveTime = 0);
	void MoveAll(uint16_t position, uint16_t moveTime = 0);
	void Commit();
};

