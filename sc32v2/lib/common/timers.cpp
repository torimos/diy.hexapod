#include "timers.h"

#define TIM_CCR4(TIMx, DATA) {TIMx->regs.gen->CCR1 = DATA[0];TIMx->regs.gen->CCR2 = DATA[1];TIMx->regs.gen->CCR3 = DATA[2];TIMx->regs.gen->CCR4 = DATA[3];}

volatile uint16_t _ccrData[5][4];
timer_dev* timers_map[] {TIMER4, TIMER1, TIMER8, TIMER3, TIMER2 };
uint32_t system_ticks = 0;

void Timer0_IrqHandler();
void Timer1_IrqHandler();
void Timer2_IrqHandler();
void Timer3_IrqHandler();
void Timer4_IrqHandler();

void __attribute__((weak)) timerHandler(uint8_t id, uint16_t *pwmData)
{
}

void initServos(int period)
{
  int timer_pins_map[] = {/*T4*/PB9,PB8,PB7,PB6,  /*T1*/PA11,PA10,PA9,PA8,  /*T8*/PC9,PC8,PC7,PC6,  /*T3*/PB1,PB0,PA7,PA6,  /*T2*/PA3,PA2,PA1,PA0 };
  voidFuncPtr timers_handlers[] { Timer0_IrqHandler, Timer1_IrqHandler, Timer2_IrqHandler, Timer3_IrqHandler, Timer4_IrqHandler };
  for(int i=0;i<20;i++)
  {
    int pin = timer_pins_map[i];
    gpio_set_mode(PIN_MAP[pin].gpio_device, PIN_MAP[pin].gpio_bit, GPIO_AF_OUTPUT_PP);
  }
  for (int i=0;i<5;i++)
  {
    timer_dev* dev = timers_map[i];
    timer_set_prescaler(dev, (F_CPU / 1000000) - 1);
    timer_set_reload(dev, period - 1);
    timer_set_compare(dev, 1, 0);
    timer_set_compare(dev, 2, 0);
    timer_set_compare(dev, 3, 0);
    timer_set_compare(dev, 4, 0);
    timer_set_mode(dev, 1, TIMER_PWM );
    timer_set_mode(dev, 2, TIMER_PWM );
    timer_set_mode(dev, 3, TIMER_PWM );
    timer_set_mode(dev, 4, TIMER_PWM );
    timer_attach_interrupt(dev, TIMER_UPDATE_INTERRUPT, timers_handlers[i]);
  }
}
void clock_IrqHandler(){
}
void clockInit(uint16_t period)
{
  timer_dev* dev = TIMER6;
  timer_set_mode(dev, 0, TIMER_OUTPUT_COMPARE);
  timer_set_prescaler(dev, (F_CPU / 1000000) - 1);
  timer_set_reload(dev, period - 1);
  timer_set_compare(dev, 0, 0);
  timer_attach_interrupt(dev, TIMER_UPDATE_INTERRUPT, clock_IrqHandler);
}

void Timer0_IrqHandler(){
  timerHandler(0, (uint16_t *)_ccrData[0]);
  TIM_CCR4(timers_map[0], _ccrData[0]);
}
void Timer1_IrqHandler(){
  timerHandler(1, (uint16_t *)_ccrData[1]);
  TIM_CCR4(timers_map[1], _ccrData[1]);
}
void Timer2_IrqHandler(){
  timerHandler(2, (uint16_t *)_ccrData[2]);
  TIM_CCR4(timers_map[2], _ccrData[2]);
}
void Timer3_IrqHandler(){
  timerHandler(3, (uint16_t *)_ccrData[3]);
  TIM_CCR4(timers_map[3], _ccrData[3]);
}
void Timer4_IrqHandler(){
  timerHandler(4, (uint16_t *)_ccrData[4]);
  TIM_CCR4(timers_map[4], _ccrData[4]);
}