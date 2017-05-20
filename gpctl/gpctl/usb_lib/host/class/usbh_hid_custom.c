#include "usbh_hid_custom.h"


static void HID_CUSTOM_Init (void);
static void HID_CUSTOM_Decode(uint8_t *data, uint16_t length);
 
HID_cb_TypeDef HID_CUSTOM_cb =
{
		HID_CUSTOM_Init,
		HID_CUSTOM_Decode,
};

static void HID_CUSTOM_Init(void)
{
	USR_CUSTOM_HID_Init();
}

static void HID_CUSTOM_Decode(uint8_t *data, uint16_t length)
{
	USR_CUSTOM_HID_ProcessData(data, length);
}
