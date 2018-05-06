#pragma once
#include "GamePadState.h"
#include "HexModel.h"

class SerialInputDriver
{
	enum RX_STATE {
		SEARCHING_SOF,
		RECEIVING_PAYLOAD
	};
	int fd;
	GamePadState state;
	GamePadState prev_state;
	
	uint8_t rx_buffer[8] = {};
	int rx_state;
	int rx_header_bytes = 0;
	int rx_payload_bytes = 0;
	int rx_errors = 0;

public:
	SerialInputDriver(const char* device, int baud);
	~SerialInputDriver();
	bool ProcessInput(HexModel* model);
	void Debug();
	bool Terminate;
private:
	uint64_t readInput(int fd);
	GamePadState parseInput(uint64_t rawState);
	bool isButtonPressed(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds = 0);
	bool isButtonPressedOnly(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds = 0);
	bool hasPressed(GamepadButtonFlags button);
	bool hasPressedOnly(GamepadButtonFlags button);
	void turnOff(HexModel* model);
	void adjustLegPositionsToBodyHeight(HexModel* model);
	int rx_pool(uint8_t data);
};

