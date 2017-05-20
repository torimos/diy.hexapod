#include "usb_bsp.h"
#ifdef USE_DEVICE_MODE
#include "usb_dcd_int.h"
#include "usb_device.h"
#endif
#ifdef USE_HOST_MODE
#include "usb_hcd_int.h"
#include "usb_host.h"
#endif

extern void USB_OTG_BSP_TimerIRQ (void);

#ifdef USE_USB_OTG_HS
void OTG_HS_IRQHandler(void)
#else
void OTG_FS_IRQHandler(void)
#endif
{
#ifdef USE_HOST_MODE
	USBH_OTG_ISR_Handler (&USB_OTG_FS_dev);
#endif
#ifdef USE_DEVICE_MODE
	USBD_OTG_ISR_Handler (&_usbDevice);
#endif
}

#ifdef USE_ACCURATE_TIME

void TIM2_IRQHandler(void)
{
	USB_OTG_BSP_TimerIRQ();
}

#endif

