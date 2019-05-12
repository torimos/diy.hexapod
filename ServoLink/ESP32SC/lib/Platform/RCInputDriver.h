#pragma once
#include "Platform.h"
#include "InputDriver.h"
#include "HexModel.h"

#define RC_NUM_CHANNELS 6
#define RC_KEEPALIVE_TIMEOUT 200

class RCInputDriver : public InputDriver
{
    typedef struct {
        uint8_t input_pin;
        uint32_t start;
        uint16_t shared;
        unsigned long keepAlive;
    } CalcParam_t;
    CalcParam_t CalcParam(uint8_t pin) 
    {
        CalcParam_t c;
        c.input_pin = pin;
        return c;
    }
    CalcParam_t rc_ch[RC_NUM_CHANNELS];

    uint16_t rc_values[RC_NUM_CHANNELS];
    bool failSafe = false;

    static void input_loop(void* arg);
    static void calc_ch(void* arg);
public:
    RCInputDriver();
    void Setup();
    bool IsTerminate();
    bool ProcessInput(HexModel* model);
	void Debug(bool clear = false);
};