#pragma once
#include "Platform.h"
#include "InputDriver.h"
#include "HexModel.h"
/*
#define RC_NUM_CHANNELS 6
#define RC_KEEPALIVE_TIMEOUT 200

#define RC_MOD  0 // RC Mode
#define RC_PWR  1 // Power On/Off
#define RC_LX   2 // Left Stick Horiz 
#define RC_SA   2 // [ALT] Left Stick Horiz
#define RC_LY   3 // Left Stick Vert 
#define RC_SB   3 // [ALT] Left Stick Vert 
#define RC_RX   4 // Right Stick Horiz
#define RC_SC   4 // [ALT] Right Stick Horiz
#define RC_RY   5 // Right Stick Vert
#define RC_SD   5 // [ALT] Right Stick Vert

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

    int32_t raw[RC_NUM_CHANNELS];
    void Reset();
    bool IsEmpty();
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
    RCInputState_t state;
    RCInputState_t prev_state;

    static void input_loop(void* arg);
    static void calc_ch(void* arg);
    void turnOff(HexModel* model);
    void adjustLegPositionsToBodyHeight(HexModel* model);
    void captureState(RCInputState_t* s);
    RCInputState_t copyState(RCInputState_t *s);
public:
    RCInputDriver();
    void Setup();
    bool IsTerminate();
    bool ProcessInput(HexModel* model);
	void Debug(bool clear = false);
};*/