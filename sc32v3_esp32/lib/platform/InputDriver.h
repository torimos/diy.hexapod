#pragma once
#include <Arduino.h>
#include "HexModel.h"

class InputDriver
{
public:
	virtual bool ProcessInput(HexModel* model) = 0;
	virtual void Debug(bool clear = false) = 0;
	virtual bool IsTerminate() = 0;
};