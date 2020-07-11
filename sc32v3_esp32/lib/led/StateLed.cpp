#include "StateLed.h"

#define LED_PIN 12
#define BRIGHTNESS 255
#define CURRENT_LIMIT 2000
#define COLOR_ORDER GRB
CRGB leds[1];

StateLedClass::StateLedClass()
{
    FastLED.addLeds<WS2812B, LED_PIN, COLOR_ORDER>(leds, 1);
    FastLED.setBrightness(BRIGHTNESS);
    if (CURRENT_LIMIT > 0) 
        FastLED.setMaxPowerInVoltsAndMilliamps(5, CURRENT_LIMIT);
}

void StateLedClass::Set(CRGB color)
{
    leds[0] = color;
    FastLED.show();
}

void StateLedClass::Flash(CRGB color, int count, int ms)
{
    for (int i=0;i<count;i++) {
        this->Set(color);
        delay(ms/2);
        this->Set(0);
        delay(ms/2);
    }
}

StateLedClass StateLed = StateLedClass();