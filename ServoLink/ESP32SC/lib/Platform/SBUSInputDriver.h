#pragma once
#include "Platform.h"
#include "InputDriver.h"
#include "HexModel.h"
#include <SBUS.h>

#define RC_NUM_CHANNELS 16
#define RC_VAL_MIN 172
#define RC_VAL_MID (992-RC_VAL_MIN)
#define RC_VAL_MAX (1811-RC_VAL_MIN)

#define RC_LX   0 // Left Stick Horiz 
#define RC_LY   1 // Left Stick Vert 
#define RC_RX   2 // Right Stick Horiz
#define RC_RY   3 // Right Stick Vert

#define RC_SF   4  // Power On/Off
#define RC_SH   5  //
#define RC_SA   6  //
#define RC_SB   7  //
#define RC_SC   8  //
#define RC_SD   9  //
#define RC_S1   10 //
#define RC_S2   11 //

typedef struct
{
    bool isPowerOn;
    byte saState;
    byte sbState;
    byte scState;
    byte sdState;
    byte shState;

    double LeftThumbX, LeftThumbY;
    double RightThumbX, RightThumbY;
    double LeftRot, RightRot;

    uint16_t raw[RC_NUM_CHANNELS];
    bool failSafe;

    void Reset();
    bool IsEmpty();
} RCInputState_t;

class SBUSInputDriver : public InputDriver
{
    SBUS* _xmp;
    RCInputState_t state;
    RCInputState_t prev_state;

    static void input_loop(void* arg);
    void turnOff(HexModel* model);
    void adjustLegPositionsToBodyHeight(HexModel* model);
    void captureState(RCInputState_t* s, RCInputState_t* p);
    RCInputState_t copyState(RCInputState_t *s);
public:
    SBUSInputDriver(HardwareSerial& serial);
    void Setup();
    bool IsTerminate();
    bool ProcessInput(HexModel* model);
 	void Debug(bool clear = false);
};