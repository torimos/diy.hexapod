#include "Stopwatch.h"
#include <wiringPi.h>
#include <stddef.h>


Stopwatch::Stopwatch()
{
	time = millis();
}


Stopwatch::~Stopwatch()
{
}

long Stopwatch::GetElapsedMilliseconds()
{
	return millis()-time;
}


void Stopwatch::Restart()
{
	time = millis();
}

void Stopwatch::Wait(long milliseconds, void (*action)())
{
	time = millis() + milliseconds;
	do
	{
		if (action != NULL)
			action();
	} while (millis() <= time);
}