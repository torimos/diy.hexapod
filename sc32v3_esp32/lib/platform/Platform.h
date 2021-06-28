#pragma once
#include <Arduino.h>
#include <HardwareSerial.h>
#include "StateLed.h"
#include "crc.h"
#include "Stopwatch.h"

#define WRITE_TO_SC32_IN_BACKGROUND 1

#define USR_PIN 0
#define NUMBER_OF_SERVO 26

extern HardwareSerial Log;
extern HardwareSerial STM32uart;
extern HardwareSerial DEBUGuart;

#pragma pack(push, 1)

typedef struct
{
	double x;
	double y;
} XY;

typedef struct
{
	double x;
	double y;
	double z;
} XYZ;

typedef struct {
	uint32_t servos[NUMBER_OF_SERVO];
	XYZ travelLength;
    XYZ bodyPos;
    XYZ bodyRot;
	bool turnedOn;
} frame_data_t;

#pragma pack(pop)

void ResetSTM32();
void Platform_Init();