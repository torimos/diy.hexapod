#include "RCInputDriver.h"


RCInputDriver::RCInputDriver()
{
    rc_ch[0] = CalcParam(36);
    rc_ch[1] = CalcParam(35);
    rc_ch[2] = CalcParam(39);
    rc_ch[3] = CalcParam(34);
    rc_ch[4] = CalcParam(32);
    rc_ch[5] = CalcParam(33);
}

bool RCInputDriver::IsTerminate()
{
    return false;
}

bool RCInputDriver::ProcessInput(HexModel* model)
{
    return false;
}

void RCInputDriver::Debug(bool clear)
{
    if (clear)
		Log.printf("\033[%d;%dH", 0, 0);
	else
        Log.println();
    Log.printf("%s ", failSafe ? "fail" : "safe");
    for(int i=0;i<RC_NUM_CHANNELS;i++)
        Log.printf("%04d ", rc_values[i]);
}

void RCInputDriver::Setup()
{
    for (int i=0; i<RC_NUM_CHANNELS; i++) {
        pinMode(rc_ch[i].input_pin, INPUT);
        attachInterruptArg(rc_ch[i].input_pin, &calc_ch, &rc_ch[i], CHANGE);
    }
    TaskHandle_t loopTask;
    xTaskCreate(input_loop, "RCInputLoopTask", 1024, this, 1, &loopTask);
}

void RCInputDriver::calc_ch(void* arg) {  
    CalcParam_t* p = static_cast<CalcParam_t*>(arg);
    uint8_t input_pin = p->input_pin;
    if (digitalRead(input_pin) == HIGH) 
    {
        p->start= micros();
        p->keepAlive = millis();
    } 
    else 
    {
        p->shared = (uint16_t)(micros() - p->start);
    }
}

void RCInputDriver::input_loop(void* arg)
{
    RCInputDriver* pThis = static_cast<RCInputDriver*>(arg);
    while(true)
    {
        noInterrupts();
        pThis->failSafe = false;
        unsigned long now = millis();
        for (int i=0;i<RC_NUM_CHANNELS;i++)
        {
            pThis->rc_values[i] = pThis->rc_ch[i].shared;
            pThis->failSafe |= (now - pThis->rc_ch[i].keepAlive) > RC_KEEPALIVE_TIMEOUT;
        }
        interrupts();
        delay(5);
    }
}

