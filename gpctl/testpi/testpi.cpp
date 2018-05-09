#include <stdio.h>
#include <stdint.h>
#include <unistd.h>

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdarg.h>
#include <string.h>
#include <termios.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <sys/types.h>
#include <sys/stat.h>

#include <string.h>
#include <errno.h>
#include <wiringPi.h>
#include <wiringSerial.h>

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



GamePadState parseInput(uint64_t rawState)
{
	GamePadState state;
	state.RawState = rawState;
	uint16_t chk = (uint16_t)((rawState >> 48) & 0xFFF0);
	if (chk != 0xFD40) rawState = 0xFD40000080808080;
	state.Buttons = (GamepadButtonFlags)((rawState >> 32) & 0x000FFFFF);
	state.LeftThumbX = (uint8_t)(rawState & 0xFF);
	state.LeftThumbY = (uint8_t)((rawState >> 8) & 0xFF);
	state.RightThumbX = (uint8_t)((rawState >> 16) & 0xFF);
	state.RightThumbY = (uint8_t)((rawState >> 24) & 0xFF);
	return state;
}

void debug(GamePadState state)
{
	//printf("GamePad - RAW: %08x%08x\n", (unsigned int)((state.RawState >> 32) & 0xFFFFFFFF), (unsigned int)(state.RawState & 0xFFFFFFFF));
	printf("Buttons: %04x\n", state.Buttons);
	printf("Left: %03d %03d\n", state.LeftThumbX, state.LeftThumbY);
	printf("Right: %03d %03d\n", state.RightThumbX, state.RightThumbY);
}

enum RX_STATE {
	SEARCHING_SOF,
    RECEIVING_PAYLOAD
};

uint8_t rx_buffer[8] =  { };
int rx_state = RX_STATE::SEARCHING_SOF;
int rx_header_bytes = 0;
int rx_payload_bytes = 0;
int rx_errors = 0;

int rx_pool(uint8_t data)
{
	if (rx_header_bytes == 1)
	{
		rx_header_bytes = 0;
		if ((data & 0xF0) == 0x40)
		{
			rx_errors = 0;
			rx_payload_bytes = 0;
			rx_buffer[7 - (rx_payload_bytes++)] = 0xFD;
			rx_buffer[7 - (rx_payload_bytes++)] = data;
			rx_state = RX_STATE::RECEIVING_PAYLOAD;
			return 0;
		}
		else
		{
			rx_errors++;
			rx_payload_bytes = 0;
			rx_state = RX_STATE::SEARCHING_SOF;
			return 0;
		}
	}

	if (data == 0xFD)
	{
		rx_header_bytes++;
	}
	else
	{
		rx_header_bytes = 0;
	}
	switch (rx_state)
	{
	case RX_STATE::SEARCHING_SOF:
		break;
	case RX_STATE::RECEIVING_PAYLOAD:
		if (data == 0xFD) // ? start of next frame
		{
			rx_state = RX_STATE::SEARCHING_SOF;
			return 1;
		}
		else if (rx_payload_bytes > 8)// ? overflow
		{
			rx_errors++;
			rx_state = RX_STATE::SEARCHING_SOF;
		}
		else
		{
			rx_buffer[7 - (rx_payload_bytes++)] = data;
		}
		break;
	}
	return 0;
}

uint64_t readInput(int fd)
{
	int ready = rx_pool(serialGetchar(fd));
	if (ready)
	{
		uint64_t raw = *((uint64_t*)rx_buffer);
		return raw;
	}
	return 0;
}

int main(int argc, char *argv[])
{
	int n, i = 0, pps, p;
	if (wiringPiSetup() == -1)
	{
		printf("Unable to start wiringPi: %s\n", strerror(errno));
	}
	int fd;
	if ((fd = serialOpen("/dev/rfcomm0", 9600)) < 0)
	{
		printf("Unable to open serial device: %s\n", strerror(errno));
	}
	
	GamePadState state;
	int t = 0;
	pps = 0;
	p = 0;
	while (1)
	{
		if ((millis() - t) >= 1000)
		{
			pps = p;
			p = 0;
			t = millis();
		}
		
		if (serialDataAvail(fd) > 0)
		{
			uint64_t raw = readInput(fd);
			if (raw != 0)
			{
				state = parseInput(raw);
//				printf("\033c");
				int row, col;
				row = col = 0;
				printf("\033[%d;%dH", row, col);
				debug(state);
				printf("RAW: %08x%08x PPS: %03d Err: %05d\n", (unsigned int)((state.RawState >> 32) & 0xFFFFFFFF), (unsigned int)(state.RawState & 0xFFFFFFFF), pps, rx_errors);
				fflush(stdout);
				p++;
			}
		}
		
		//delay(200);
	}
	serialClose(fd);

	return 0;
}