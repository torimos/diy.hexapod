#include "core.h"


void _delay(u32 usec)
{
	uint32_t count = 0;
	const uint32_t utime = (120 * usec / 7);
	do
	{
		if ( ++count > utime )
		{
		  return;
		}
	}
	while (1);
}

void _delay_ms(u32 msec)
{
	SysTick->LOAD  = ((SystemCoreClock/1000) & SysTick_LOAD_RELOAD_Msk) - 1;
	SysTick->VAL   = 0;
	SysTick->CTRL  = SysTick_CTRL_CLKSOURCE_Msk | SysTick_CTRL_ENABLE_Msk;
	while(msec>0) if (SysTick->CTRL&SysTick_CTRL_COUNTFLAG_Msk) msec--;
	SysTick->VAL   = SysTick->CTRL  = 0;
}

void _delay_us(u32 usec)
{
	SysTick->LOAD  = ((SystemCoreClock/1000000) & SysTick_LOAD_RELOAD_Msk) - 1;
	SysTick->VAL   = 0;
	SysTick->CTRL  = SysTick_CTRL_CLKSOURCE_Msk | SysTick_CTRL_ENABLE_Msk;
	while(usec>0) if (SysTick->CTRL&SysTick_CTRL_COUNTFLAG_Msk) usec--;
	SysTick->VAL   = SysTick->CTRL  = 0;
}
