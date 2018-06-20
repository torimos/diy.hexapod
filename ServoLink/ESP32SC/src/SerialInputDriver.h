#pragma once
#include "Platform.h"
#include "GamePadState.h"
#include "HexModel.h"
#include "Stream.h"

class SerialInputDriver
{
	enum RX_STATE {
		SEARCHING_SOF,
		RECEIVING_PAYLOAD
	};
	GamePadState state;
	GamePadState prev_state;
	
	uint8_t rx_buffer[8] = {};
	int rx_state;
	int rx_header_bytes = 0;
	int rx_payload_bytes = 0;
	int rx_errors = 0;
	Stream *_stream;
public:
	SerialInputDriver(Stream* stream);
	~SerialInputDriver();
	bool ProcessInput(HexModel* model);
	void Debug();
	bool Terminate;
private:
	uint64_t readInput();
	GamePadState parseInput(uint64_t rawState);
	GamePadState processStreamInput();
	bool isButtonPressed(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds = 0);
	bool isButtonPressedOnly(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds = 0);
	bool hasPressed(GamepadButtonFlags button);
	bool hasPressedOnly(GamepadButtonFlags button);
	void turnOff(HexModel* model);
	void adjustLegPositionsToBodyHeight(HexModel* model);
	int rx_pool(uint8_t data);
};

