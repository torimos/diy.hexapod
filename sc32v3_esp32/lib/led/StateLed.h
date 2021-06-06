#pragma once
#include <Arduino.h>
#include <FastLED.h>


class StateLedClass
{
public:
    StateLedClass();
    void Set(CRGB color);
    void Flash(CRGB color, int count, int delay);
};

extern StateLedClass StateLed;