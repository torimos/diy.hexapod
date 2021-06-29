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

typedef struct {
	// 6 Legs order
	// 5 LF ^ RF 2
	// 4 LM + RM 1
	// 3 LR . RR 0
	// Each LEG composed of 3 virtual servos (tibia,femur,coxia)
	// Angle offset for each virtual servo
	short ServoOffset[18];
	// Angle inversion value for each virtual servo
	short ServoInv[18];
	// Map virtual to physical servos connected to controller
	short ServoMap[18];
} settings_t;

typedef struct {
	settings_t settings;
	bool save;
} frame_settings_data_t;

#pragma pack(pop)

void ResetSTM32();
void Platform_Init();