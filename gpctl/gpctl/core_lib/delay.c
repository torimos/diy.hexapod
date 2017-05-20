#include "delay.h"

void delay_ms(u32 ms)
{
	SysTick->LOAD  = ((SystemCoreClock/1000) & SysTick_LOAD_RELOAD_Msk) - 1;
	SysTick->VAL   = 0;
	SysTick->CTRL  = SysTick_CTRL_CLKSOURCE_Msk | SysTick_CTRL_ENABLE_Msk;
	while(ms>0) if (SysTick->CTRL&SysTick_CTRL_COUNTFLAG_Msk) ms--;
	SysTick->VAL   = SysTick->CTRL  = 0;
}

void delay_us(u32 us)
{
	SysTick->LOAD  = ((SystemCoreClock/1000000) & SysTick_LOAD_RELOAD_Msk) - 1;
	SysTick->VAL   = 0;
	SysTick->CTRL  = SysTick_CTRL_CLKSOURCE_Msk | SysTick_CTRL_ENABLE_Msk;
	while(us>0) if (SysTick->CTRL&SysTick_CTRL_COUNTFLAG_Msk) us--;
	SysTick->VAL   = SysTick->CTRL  = 0;
}
