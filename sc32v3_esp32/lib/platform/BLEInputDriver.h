#pragma once
#include "Platform.h"
#include "HexModel.h"
#include "Stream.h"
#include "InputDriver.h"

#pragma pack(push, 1)
typedef struct
{
    uint8_t status;
    uint32_t buttons;
    int16_t axis_data[4];
} joy_data_t;
typedef struct
{
    int16_t mag_data[3];
    int16_t accel_data[3];
    int16_t gyro_data[3];
} sens_data_t;
#pragma pack(pop)

typedef struct
{
    uint32_t buttons;
    int16_t axis_data[4];
	bool initialized;
} input_state_t;

class BLEInputDriver : public InputDriver
{
	input_state_t state;
	input_state_t prev_state;
	
	bool _terminate;
public:
	BLEInputDriver();
	~BLEInputDriver();
	void Setup();
	bool ProcessInput(HexModel* model);
	void Debug(bool clear = false);
	bool IsTerminate() { return _terminate; }

	bool incomingDataProcessed;
	joy_data_t incomingData;
private:
	bool isButtonPressed(input_state_t state, uint32_t flag, int delayMilliseconds = 0);
	bool isButtonPressedOnly(input_state_t state, uint32_t flag, int delayMilliseconds = 0);
	bool hasPressed(uint32_t button);
	bool hasPressedOnly(uint32_t button);
	void turnOff(HexModel* model);
	void adjustLegPositionsToBodyHeight(HexModel* model);
};

