#include "timers.h"
#include "logger.h"

#define TIM_CCR4(TIMx, DATA) {TIMx->regs.gen->CCR1 = DATA[0];TIMx->regs.gen->CCR2 = DATA[1];TIMx->regs.gen->CCR3 = DATA[2];TIMx->regs.gen->CCR4 = DATA[3];}
void Timer0_IrqHandler();
void Timer1_IrqHandler();
void Timer2_IrqHandler();
void Timer3_IrqHandler();
void Timer4_IrqHandler();
void Timer5_IrqHandler();
void Timer6_IrqHandler();

int pwm_pins_map[] = {/*T4*/PB9,PB8,PB7,PB6,  /*T1*/PA11,PA10,PA9,PA8,  /*T8*/PC9,PC8,PC7,PC6,  /*T3*/PB1,PB0,PA7,PA6,  /*T2*/PA3,PA2,PA1,PA0 };
int dpwm_pins_map[] = {/*TD*/PB15,PB14,PB13,PB12,PB11,PB10 };
const int channels_map[] = {/*T4*/3,2,1,0, /*T1*/7,6,5,4, /*T8*/11,10,9,8, /*T3*/15,14,13,12, /*T2*/19,18,17,16, /*TD*/20,21,22,23,24,25};

voidFuncPtr pwm_timer_handlers[] { Timer0_IrqHandler, Timer1_IrqHandler, Timer2_IrqHandler, Timer3_IrqHandler, Timer4_IrqHandler };
voidFuncPtr basic_timer_handlers[] { Timer5_IrqHandler, Timer6_IrqHandler };
timer_dev* pwm_timers_map[] { TIMER4, TIMER1, TIMER8, TIMER3, TIMER2 };
timer_dev* basic_timers_map[] { TIMER6, TIMER7 };

volatile uint16_t _pwmData[5][4];
volatile uint16_t _pwmPeriod = 0;
volatile uint16_t _dpwm_data[6] = { 0,0,0,0,0,0 };
volatile uint16_t _dpwm_step = 10;
volatile uint16_t _dpwm_counter = 0;
uint16_t _pwmZeroData[4] = {0,0,0,0};

uint16_t __attribute__((weak)) timer_getPWMValue(uint8_t sid)
{
  return 0;
}

void initServos(uint16_t period)
{
  _pwmPeriod = period;
  for(int i=0;i<20;i++)
  {
    int pin = pwm_pins_map[i];
    gpio_set_mode(PIN_MAP[pin].gpio_device, PIN_MAP[pin].gpio_bit, GPIO_AF_OUTPUT_PP);
  }
  for(int i=0;i<6;i++)
  {
    int pin = dpwm_pins_map[i];
    gpio_set_mode(PIN_MAP[pin].gpio_device, PIN_MAP[pin].gpio_bit, GPIO_OUTPUT_PP);
    gpio_write_bit(PIN_MAP[pin].gpio_device, PIN_MAP[pin].gpio_bit, 0);
  }
  delay(1);
  for (int i=0;i<5;i++)
  {
    _pwmData[i][0] = _pwmData[i][1] = _pwmData[i][2] =_pwmData[i][3] = 0;
    timer_dev* dev = pwm_timers_map[i];
    timer_set_prescaler(dev, (F_CPU / 1000000) - 1);
    timer_set_reload(dev, period - 1);
    TIM_CCR4(dev, _pwmZeroData);
    timer_set_mode(dev, 1, TIMER_PWM );
    timer_set_mode(dev, 2, TIMER_PWM );
    timer_set_mode(dev, 3, TIMER_PWM );
    timer_set_mode(dev, 4, TIMER_PWM );
    timer_attach_interrupt(dev, TIMER_UPDATE_INTERRUPT, pwm_timer_handlers[i]);
  }

  timer_dev* dev = basic_timers_map[0]; //timer for soft pwm
  timer_pause(dev);
  timer_set_prescaler(dev, (F_CPU / 1000000) - 1); // timer clock is 1 us
  timer_set_reload(dev, _dpwm_step - 1); // reload every 5 us
  timer_attach_interrupt(dev, TIMER_UPDATE_INTERRUPT, basic_timer_handlers[0]);
  timer_resume(dev);
}

void Timer0_IrqHandler(){
  for (int sid = 0; sid < 4; sid++)
    _pwmData[0][sid] = timer_getPWMValue(channels_map[sid]);
  TIM_CCR4(pwm_timers_map[0], _pwmData[0]);
}
void Timer1_IrqHandler(){
  for (int sid = 0; sid < 4; sid++)
    _pwmData[1][sid] = timer_getPWMValue(channels_map[4 + sid]);
  TIM_CCR4(pwm_timers_map[1], _pwmData[1]);
}
void Timer2_IrqHandler(){
  for (int sid = 0; sid < 4; sid++)
    _pwmData[2][sid] = timer_getPWMValue(channels_map[8 + sid]);
  TIM_CCR4(pwm_timers_map[2], _pwmData[2]);
}
void Timer3_IrqHandler(){
  for (int sid = 0; sid < 4; sid++)
    _pwmData[3][sid] = timer_getPWMValue(channels_map[12 + sid]);
  TIM_CCR4(pwm_timers_map[3], _pwmData[3]);
}
void Timer4_IrqHandler(){
  for (int sid = 0; sid < 4; sid++)
    _pwmData[4][sid] = timer_getPWMValue(channels_map[16 + sid]);
  TIM_CCR4(pwm_timers_map[4], _pwmData[4]);
}
void Timer5_IrqHandler(){
  for(int i=0;i<6;i++)
  {
    _dpwm_data[i] = timer_getPWMValue(channels_map[20 + i]);
    int pin = dpwm_pins_map[i];
    if (_dpwm_counter == 0)  
      gpio_write_bit(PIN_MAP[pin].gpio_device, PIN_MAP[pin].gpio_bit, 1);
    else if (_dpwm_counter == _dpwm_data[i])
      gpio_write_bit(PIN_MAP[pin].gpio_device, PIN_MAP[pin].gpio_bit, 0);
  }
  _dpwm_counter = (_dpwm_counter + _dpwm_step) % _pwmPeriod;
}
void Timer6_IrqHandler(){
}