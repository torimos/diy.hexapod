#ifndef __USBH_HID_CUSTOM_H
#define __USBH_HID_CUSTOM_H

#include "usbh_hid_core.h"

extern HID_cb_TypeDef HID_CUSTOM_cb;

void  USR_CUSTOM_HID_Init (void);
void  USR_CUSTOM_HID_ProcessData (uint8_t *data, uint16_t length);

#endif /* __USBH_HID_CUSTOM_H */

