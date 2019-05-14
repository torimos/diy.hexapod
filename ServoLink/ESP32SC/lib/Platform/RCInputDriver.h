#pragma once
#include "Platform.h"
#include "InputDriver.h"
#include "HexModel.h"

#define RC_NUM_CHANNELS 6
#define RC_KEEPALIVE_TIMEOUT 200

#define RC_MOD  0
#define RC_PWR  1
#define RC_LX  2
#define RC_LY  3
#define RC_RX  4
#define RC_RY  5
#define RC_SA  2
#define RC_SB  3
#define RC_SC  4
#define RC_SD  5

typedef struct
{
    bool pwrHasChanged;
    bool pwrIsOn;
    bool modeHasChanged;
    bool modeIsOn;

    bool saHasChanged;
    char saState;
    bool sbHasChanged;
    char sbState;
    bool scHasChanged;
    char scState;
    bool sdHasChanged;
    char sdState;

    double LeftThumbX, LeftThumbY;
    double RightThumbX, RightThumbY;

    void Reset();
} RCInputState_t;

class RCInputDriver : public InputDriver
{
    class RCChannel
    {
        uint8_t  _input_pin;
        unsigned long _start;
        uint32_t  _duration;
        uint32_t _value;
        unsigned long  _timestamp;
    public:
        RCChannel(uint8_t pin)
        { 
            _input_pin = pin;
            _duration = _value = _start = 0;
        }
        uint16_t pin() { return _input_pin; }
        uint32_t start() { _timestamp = millis(); return _start =  micros(); }
        uint32_t end() { return _duration =  (uint32_t)(micros() - _start); }
        uint32_t timestamp() { return _timestamp; }
        uint32_t value(bool refresh = false) { 
            if (refresh) {
                _value = _duration;
            }
            return _value; 
        }
    };

    RCChannel* rc_ch[RC_NUM_CHANNELS];
    bool failSafe = false;
    RCInputState_t prevInputState;

    static void input_loop(void* arg);
    static void calc_ch(void* arg);
    void turnOff(HexModel* model);
    void adjustLegPositionsToBodyHeight(HexModel* model);
    RCInputState_t captureState(RCInputState_t prev);
    RCInputState_t copyState(RCInputState_t s);
public:
    RCInputDriver();
    void Setup();
    bool IsTerminate();
    bool ProcessInput(HexModel* model);
	void Debug(bool clear = false);
};