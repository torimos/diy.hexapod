#include "core.h"
#include "usb_host.h"
#include "usbh_hid_core.h"

typedef struct
{
	unsigned char LeftThumbX;
	unsigned char LeftThumbY;
	unsigned char RightThumbX;
	unsigned char RightThumbY;
	unsigned int Buttons;
} State;

typedef enum
{
	None = 0,
	DPadUp = 1,
	DPadRight = 2,
	DPadDown = 4,
	DPadLeft = 8,
	B1 = 0x10,
	B2 = 0x20,
	B3 = 0x40,
	B4 = 0x80,
	B5 = 0x100,
	B6 = 0x200,
	B7 = 0x400,
	B8 = 0x800,
	B9 = 0x1000,
	B10 = 0x2000,
	LeftThumb = 0x4000,
	RightThumb = 0x8000,
	Vibration = 0x40000,
	Mode = 0x80000
} ButtonType;

ButtonType DPad[] = {
	DPadUp,
	DPadUp | DPadRight,
	DPadRight,
	DPadRight | DPadDown,
	DPadDown,
	DPadDown | DPadLeft,
	DPadLeft,
	DPadLeft | DPadUp,
	None
};

const unsigned int GPIDC = 0xFD400000;

void USB_DataReceived(uint8_t *data, uint16_t length)
{
	State* s = (State*)data;
	s->Buttons = GPIDC | (DPad[s->Buttons & 0xF] | (s->Buttons & 0x000FFFF0));
#ifdef DEBUG
	printf("%08X %03X %03X %03X %03X \n\r", s->Buttons, s->LeftThumbX, s->LeftThumbY, s->RightThumbX, s->RightThumbY);
#else
	uart_send_buffer(USART3, data, length);
#endif
}
void USB_Initialized()
{
#ifdef DEBUG
	printf("Connected to usb controller\n\r");
#endif
}

void USB_EnumerationDone(USBH_DeviceProp_TypeDef* dev, USBH_DevDesc_TypeDef* desc)
{
#ifdef DEBUG
	if (dev->speed == HPRT0_PRTSPD_HIGH_SPEED)
	{
		printf("HIGH SPEED Device at %d\n\r", dev->address);
	}
	else if (dev->speed == HPRT0_PRTSPD_FULL_SPEED)
	{
		printf("FULL SPEED Device at %d\n\r", dev->address);
	}
	else if (dev->speed == HPRT0_PRTSPD_LOW_SPEED)
	{
		printf("LOW SPEED Device at %d\n\r", dev->address);
	}
	else
	{
		printf("ERROR: Device Speed Unknown at %d\n\r", dev->address);
	}

	printf("VID:%04X PID:%04X RID:%04X USB:%04X\n\r", desc->idVendor, desc->idProduct, desc->bcdDevice, desc->bcdUSB);
	printf("MPSZ:%02X #CFG:%02X CSR:%02X%02X%02X #INT:%d\n\r", desc->bMaxPacketSize, desc->bNumConfigurations, desc->bDeviceClass, desc->bDeviceSubClass, desc->bDeviceProtocol, dev->Cfg_Desc.bNumInterfaces);
	u8 i = 0, j;
	for (; i < dev->Cfg_Desc.bNumInterfaces; i++)
	{
		USBH_InterfaceDesc_TypeDef* id = &dev->Itf_Desc;//[i] - single
		printf("\tINT%02X> AS:%02X CSR:%02X%02X%02X #EP:%02X\n\r", i, id->bAlternateSetting, id->bInterfaceClass, id->bInterfaceSubClass, id->bInterfaceProtocol, id->bNumEndpoints);
		for (j = 0; j < id->bNumEndpoints; j++)
		{
			USBH_EpDesc_TypeDef *pep = &dev->Ep_Desc[j];//[i]
			printf("\t\tEP%02X> ADDR:%02X MPSZ:%04X PI:%02X ATTR:%02X\n\r", j, pep->bEndpointAddress, pep->wMaxPacketSize, pep->bInterval, pep->bmAttributes);
		}
	}
#endif
}
	
int main(void)
{
	USBH_HID_Init();
	uart_init(USART3);
#ifdef DEBUG
	printf("\tLogitech Wirelles Controler V1.2\n\r");
#endif		while (1)
	{
		USBH_HID_Process();
	}
}
