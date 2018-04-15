#pragma once
#include <stdint.h>
typedef enum 
{
	None       = 0,
	DPadUp     = 1,
	DPadRight  = 2,
	DPadDown   = 4,
	DPadLeft   = 8,
	B1         = 0x10,
	B2         = 0x20,
	B3         = 0x40,
	B4         = 0x80,
	B5         = 0x100,
	B6         = 0x200,
	B7         = 0x400,
	B8         = 0x800,
	B9         = 0x1000,
	B10        = 0x2000,
	LeftThumb  = 0x4000,
	RightThumb = 0x8000,
	Vibration  = 0x40000,
	Mode       = 0x80000
} GamepadButtonFlags;

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
