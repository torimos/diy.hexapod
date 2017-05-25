#include "usbh_core.h"
#include "usbh_hid_core.h"
#include "usbh_hid_custom.h"
#include "usbh_hid_keybd.h"
#include "usbh_hid_mouse.h"
#include <stdio.h>

void USBH_USR_Init(void);
void USBH_USR_DeviceAttached(void);
void USBH_USR_ResetDevice(void);
void USBH_USR_DeviceDisconnected (void);
void USBH_USR_OverCurrentDetected (void);
void USBH_USR_DeviceSpeedDetected(uint8_t DeviceSpeed);
void USBH_USR_Device_DescAvailable(void *);
void USBH_USR_DeviceAddressAssigned(void);
void USBH_USR_Configuration_DescAvailable(USBH_CfgDesc_TypeDef * cfgDesc,
                                          USBH_InterfaceDesc_TypeDef *itfDesc,
                                          USBH_EpDesc_TypeDef *epDesc);
void USBH_USR_Manufacturer_String(void *);
void USBH_USR_Product_String(void *);
void USBH_USR_SerialNum_String(void *);
USBH_USR_Status USBH_USR_UserInput(void);
void USBH_USR_EnumerationDone(void);
void USBH_USR_DeviceNotSupported(void);
void USBH_USR_UnrecoveredError(void);

USBH_Usr_cb_TypeDef USR_Callbacks =
{
	USBH_USR_Init,
	USBH_USR_DeviceAttached,
	USBH_USR_ResetDevice,
	USBH_USR_DeviceDisconnected,
	USBH_USR_OverCurrentDetected,
	USBH_USR_DeviceSpeedDetected,
	USBH_USR_Device_DescAvailable,
	USBH_USR_DeviceAddressAssigned,
	USBH_USR_Configuration_DescAvailable,
	USBH_USR_Manufacturer_String,
	USBH_USR_Product_String,
	USBH_USR_SerialNum_String,
	USBH_USR_EnumerationDone,
	USBH_USR_UserInput,
	USBH_USR_DeviceNotSupported,
	USBH_USR_UnrecoveredError
};

///////////////////////////////////////////////////////////////////////

void USBH_HID_Init()
{
	USBH_Init(&USB_OTG_FS_dev, &HID_cb , &USR_Callbacks);
}
void USBH_HID_Process()
{
	USBH_Process();
}

///////////////////////////////////////////////////////////////////////

void USBH_USR_Init(void)
{
}
void USBH_USR_DeviceAttached(void)
{
}
void USBH_USR_UnrecoveredError (void)
{
}
void USBH_USR_DeviceDisconnected (void)
{
}
void USBH_USR_ResetDevice(void)
{
}
void USBH_USR_DeviceSpeedDetected(uint8_t DeviceSpeed)
{
  if (DeviceSpeed == HPRT0_PRTSPD_FULL_SPEED)
  {
  }
  else if (DeviceSpeed == HPRT0_PRTSPD_LOW_SPEED)
  {
  }
  else
  {
  }
}
void USBH_USR_Device_DescAvailable(void *DeviceDesc)
{
}
void USBH_USR_DeviceAddressAssigned(void)
{
}
void USBH_USR_Configuration_DescAvailable(USBH_CfgDesc_TypeDef * cfgDesc,
    USBH_InterfaceDesc_TypeDef *itfDesc,
    USBH_EpDesc_TypeDef *epDesc)
{
}
void USBH_USR_Manufacturer_String(void *ManufacturerString)
{
}
void USBH_USR_Product_String(void *ProductString)
{
}
void USBH_USR_SerialNum_String(void *SerialNumString)
{
}
void USBH_USR_DeviceNotSupported(void)
{
}
USBH_USR_Status USBH_USR_UserInput(void)
{
  return USBH_USR_RESP_OK;
}
void USBH_USR_OverCurrentDetected (void)
{
}
void USBH_USR_EnumerationDone(void)
{
	USBH_DeviceProp_TypeDef* pdev = &USBH_Device;
	USBH_DevDesc_TypeDef* dd = &pdev->Dev_Desc;

	USB_EnumerationDone(pdev, dd);
}

void  USR_KEYBRD_Init (void)
{
}
void  USR_KEYBRD_ProcessData (uint8_t pbuf)
{
}

void  USR_MOUSE_Init (void)
{
}
void  USR_MOUSE_ProcessData (HID_MOUSE_Data_TypeDef *data)
{
}

void  USR_CUSTOM_HID_Init (void)
{
	USB_Initialized();
}

void USR_CUSTOM_HID_ProcessData (uint8_t *data, uint16_t length)
{
	USB_DataReceived(data, length);
}
