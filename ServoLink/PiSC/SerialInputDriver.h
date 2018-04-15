#pragma once
#include "GamePadState.h"
#include "HexModel.h"

class SerialInputDriver
{
	int fd;
	GamePadState state;
	GamePadState prev_state;
public:
	SerialInputDriver(const char* device);
	~SerialInputDriver();
	bool ProcessInput(HexModel* model);
	void Debug();
	bool Terminate;
private:
	static uint64_t readInput(int fd);
	static GamePadState parseInput(uint64_t rawState);
	bool isButtonPressed(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds = 0);
	bool isButtonPressedOnly(GamePadState state, GamepadButtonFlags flag, int delayMilliseconds = 0);
	bool hasPressed(GamepadButtonFlags button);
	bool hasPressedOnly(GamepadButtonFlags button);
	void turnOff(HexModel* model);
	void adjustLegPositionsToBodyHeight(HexModel* model);
};

