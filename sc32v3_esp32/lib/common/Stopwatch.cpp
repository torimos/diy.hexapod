#include "Arduino.h"
#include "Stopwatch.h"
#include <stddef.h>


Stopwatch::Stopwatch()
{
	time = millis();
}


Stopwatch::~Stopwatch()
{
}

unsigned long Stopwatch::GetElapsedMilliseconds()
{
	return millis()-time;
}


void Stopwatch::Restart()
{
	time = millis();
}

void Stopwatch::Wait(unsigned long milliseconds, void (*action)())
{
	time = millis() + milliseconds;
	do
	{
		if (action != NULL)
			action();
	} while (millis() <= time);
}