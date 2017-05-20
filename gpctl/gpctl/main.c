#include "core.h"
#include "usb_host.h"
#include "usbh_hid_core.h"
void USB_DataReceived(uint8_t *data, uint16_t length)
{
	uart_send_buffer(USART3, data, length);
}
int main(void)
{
	gpio_init(GPIOB, GPIO_Pin_6, GPIO_Mode_Out_PP, GPIO_Speed_50MHz);
	gpio_write(GPIOB, GPIO_Pin_6, SET);//VBUS

	uart_init(USART3);

	USBH_HID_Init();

	while (1)
	{
		USBH_HID_Process();
	}
}
