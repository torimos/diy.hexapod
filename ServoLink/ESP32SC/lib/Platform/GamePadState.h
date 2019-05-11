#pragma once
#include <stdint.h>
enum GamepadButtonFlags
{
	None       = 0,
	DPadUp     = 1,
	DPadRight  = 2,
	DPadDown   = 4,
	DPadLeft   = 8,
	Btn1       = 0x10,
	Btn2       = 0x20,
	Btn3       = 0x40,
	Btn4       = 0x80,
	Btn5       = 0x100,
	Btn6       = 0x200,
	Btn7       = 0x400,
	Btn8       = 0x800,
	Btn9       = 0x1000,
	Btn10      = 0x2000,
	LeftThumb  = 0x4000,
	RightThumb = 0x8000,
	Vibration  = 0x40000,
	Mode       = 0x80000
};

typedef struct 
{
	int LeftThumbX;
	int LeftThumbY;
	int RightThumbX;
	int RightThumbY;
	GamepadButtonFlags Buttons;
	uint64_t RawState;
} GamePadState;

template<class T> inline T operator~(T a) { return (T)~(int)a; }
template<class T> inline T operator|(T a, T b) { return (T)((int)a | (int)b); }
template<class T> inline T operator&(T a, T b) { return (T)((int)a & (int)b); }
template<class T> inline T operator^(T a, T b) { return (T)((int)a ^ (int)b); }
template<class T> inline T& operator|=(T& a, T b) { return (T&)((int&)a |= (int)b); }
template<class T> inline T& operator&=(T& a, T b) { return (T&)((int&)a &= (int)b); }
template<class T> inline T& operator^=(T& a, T b) { return (T&)((int&)a ^= (int)b); }
