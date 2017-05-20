#ifndef __USB_HOST_H__
#define __USB_HOST_H__

#include "usbh_core.h"

void USBH_HID_Init();
void USBH_HID_Process();

extern void USB_DataReceived(uint8_t *data, uint16_t length);

#endif // __USB_HOST_H__
