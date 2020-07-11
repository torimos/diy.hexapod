#pragma once
class Stopwatch
{
	unsigned long time;
public:
	Stopwatch();
	~Stopwatch();
	void Restart();
	unsigned long GetElapsedMilliseconds();
	void Wait(unsigned long milliseconds, void (*action)());
};

