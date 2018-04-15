#include "Stopwatch.h"
#include <wiringPi.h>

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