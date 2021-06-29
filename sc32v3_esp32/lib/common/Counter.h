#pragma once
#include "Stopwatch.h"

class Counter
{
    Stopwatch* sw;
    int ticks;
    int ticksPerSecond;
public:
    Counter(){
        ticksPerSecond = ticks = 0;
        sw = new Stopwatch();
    }
    ~Counter(){
        delete sw;
    }

    int GetTicksPerSecond()
    {
        return ticksPerSecond;
    }

    void Tick()
    {
        if (sw->GetElapsedMilliseconds() >= 1000)
        {
            ticksPerSecond = ticks;
            ticks = 0;
            sw->Restart();
        }
        ticks++;
    }

    void Reset()
    {
        ticksPerSecond = ticks = 0;
        sw->Restart();
    }
};