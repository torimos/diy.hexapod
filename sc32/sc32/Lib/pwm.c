#include "pwm.h"
#define PWM_MIN 0 //600
#define PWM_MID (PWM_MIN+PWM_RANGE/2)
#define PWM_PERIOD 20000
#define TIM_CCR4(TIMx, VAL1, VAL2, VAL3, VAL4) {TIMx->CCR1 = PWM_MIN+VAL1;TIMx->CCR2 = PWM_MIN+VAL2;TIMx->CCR3 = PWM_MIN+VAL3;TIMx->CCR4 = PWM_MIN+VAL4;}
#define TIM_CCR2(TIMx, VAL1, VAL2) {TIMx->CCR1 = PWM_MIN+VAL1;TIMx->CCR2 = PWM_MIN+VAL2;}

TIM_TypeDef *timers[6] = {TIM1,TIM2,TIM3,TIM4,TIM8,TIM12};
volatile uint16_t* pwm_data[6];

void TimerOutputInit(TIM_TypeDef* TIMx, uint8_t channel, uint16_t pulse)
{
	TIM_OCInitTypeDef timerOutput;
	timerOutput.TIM_OCMode = TIM_OCMode_PWM1;
	timerOutput.TIM_OutputState = TIM_OutputState_Enable;
	timerOutput.TIM_OutputNState = TIM_OutputNState_Disable;
	timerOutput.TIM_OCPolarity = TIM_OCPolarity_High;
	timerOutput.TIM_OCNPolarity = TIM_OCPolarity_High;
	timerOutput.TIM_OCIdleState = TIM_OCIdleState_Reset;
	timerOutput.TIM_OCNIdleState = TIM_OCIdleState_Reset;
	timerOutput.TIM_Pulse = pulse;
	switch(channel%4)
	{
	case 0:
		TIM_OC1Init(TIMx, &timerOutput);
		TIM_OC1PreloadConfig(TIMx, TIM_OCPreload_Enable);
		break;
	case 1:
		TIM_OC2Init(TIMx, &timerOutput);
		TIM_OC2PreloadConfig(TIMx, TIM_OCPreload_Enable);
		break;
	case 2:
		TIM_OC3Init(TIMx, &timerOutput);
		TIM_OC3PreloadConfig(TIMx, TIM_OCPreload_Enable);
		break;
	case 3:
		TIM_OC4Init(TIMx, &timerOutput);
		TIM_OC4PreloadConfig(TIMx, TIM_OCPreload_Enable);
		break;
	}
//	if (TIMx == TIM1 || TIMx == TIM8)
//	{
//		TIM_BDTRInitTypeDef TIM_BDTRInitStructure;
//		 /* Automatic Output enable, Break, dead time and lock configuration*/
//		TIM_BDTRInitStructure.TIM_OSSRState = TIM_OSSRState_Enable;
//		TIM_BDTRInitStructure.TIM_OSSIState = TIM_OSSIState_Enable;
//		TIM_BDTRInitStructure.TIM_LOCKLevel = TIM_LOCKLevel_1;
//		TIM_BDTRInitStructure.TIM_DeadTime = 117;
//		TIM_BDTRInitStructure.TIM_Break = TIM_Break_Enable;
//		TIM_BDTRInitStructure.TIM_BreakPolarity = TIM_BreakPolarity_High;
//		TIM_BDTRInitStructure.TIM_AutomaticOutput = TIM_AutomaticOutput_Enable;
//		TIM_BDTRConfig(TIMx, &TIM_BDTRInitStructure);
//
//	}
}

void NVICInit(TIM_TypeDef* TIMx)
{
	NVIC_InitTypeDef NVIC_InitStructure;
	NVIC_PriorityGroupConfig(NVIC_PriorityGroup_0);
	if (TIMx == TIM1)
		NVIC_InitStructure.NVIC_IRQChannel = TIM1_UP_TIM10_IRQn;
	else if (TIMx == TIM2)
		NVIC_InitStructure.NVIC_IRQChannel = TIM2_IRQn;
	else if (TIMx == TIM3)
		NVIC_InitStructure.NVIC_IRQChannel = TIM3_IRQn;
	else if (TIMx == TIM4)
		NVIC_InitStructure.NVIC_IRQChannel = TIM4_IRQn;
	else if (TIMx == TIM8)
		NVIC_InitStructure.NVIC_IRQChannel = TIM8_UP_TIM13_IRQn;
	else if (TIMx == TIM12)
		NVIC_InitStructure.NVIC_IRQChannel = TIM8_BRK_TIM12_IRQn;
	NVIC_InitStructure.NVIC_IRQChannelPreemptionPriority = 0;
	NVIC_InitStructure.NVIC_IRQChannelSubPriority = 0;
	NVIC_InitStructure.NVIC_IRQChannelCmd = ENABLE;
	NVIC_Init(&NVIC_InitStructure);
	TIM_ITConfig(TIMx, TIM_IT_Update, ENABLE);
}

void TimerGPIOInit(TIM_TypeDef* TIMx)
{
	RCC_APB2PeriphClockCmd(RCC_APB2Periph_TIM1 | RCC_APB2Periph_TIM8 | RCC_APB2Periph_GPIOA | RCC_APB2Periph_GPIOB | RCC_APB2Periph_GPIOC | RCC_APB2Periph_GPIOD | RCC_APB2Periph_AFIO, ENABLE);
	RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM2 | RCC_APB1Periph_TIM3 |RCC_APB1Periph_TIM4 | RCC_APB1Periph_TIM12, ENABLE);

	RCC_APB2PeriphClockCmd(RCC_APB2Periph_AFIO, ENABLE);
	GPIO_PinRemapConfig(GPIO_Remap_SWJ_JTAGDisable, ENABLE);

	GPIO_InitTypeDef portInit;
	portInit.GPIO_Speed = GPIO_Speed_2MHz;
	portInit.GPIO_Mode  = GPIO_Mode_AF_PP;
	if (TIMx == TIM1)
	{
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_TIM1 | RCC_APB2Periph_GPIOA, ENABLE);
		portInit.GPIO_Pin = GPIO_Pin_8 | GPIO_Pin_9 | GPIO_Pin_10 | GPIO_Pin_11;
		GPIO_Init(GPIOA, &portInit);
	}
	else if (TIMx == TIM2)
	{
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOA, ENABLE);
		RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM2, ENABLE);
		portInit.GPIO_Pin   = GPIO_Pin_0 | GPIO_Pin_1 | GPIO_Pin_2 | GPIO_Pin_3;
		GPIO_Init(GPIOA, &portInit);
	}
	else if (TIMx == TIM3)
	{
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOA | RCC_APB2Periph_GPIOB, ENABLE);
		RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM3, ENABLE);
		portInit.GPIO_Pin   = GPIO_Pin_6 | GPIO_Pin_7;
		GPIO_Init(GPIOA, &portInit);
		portInit.GPIO_Pin   = GPIO_Pin_0 | GPIO_Pin_1;
		GPIO_Init(GPIOB, &portInit);
	}
	else if (TIMx == TIM4)
	{
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOB, ENABLE);
		RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM4, ENABLE);
		portInit.GPIO_Pin   = GPIO_Pin_6 | GPIO_Pin_7 | GPIO_Pin_8 | GPIO_Pin_9;
		GPIO_Init(GPIOB, &portInit);
	}
	else if (TIMx == TIM8)
	{
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_TIM8 | RCC_APB2Periph_GPIOC, ENABLE);
		portInit.GPIO_Pin   = GPIO_Pin_6 | GPIO_Pin_7 | GPIO_Pin_8 | GPIO_Pin_9;
		GPIO_Init(GPIOC, &portInit);
	}
	else if (TIMx == TIM12)
	{
		RCC_APB2PeriphClockCmd(RCC_APB2Periph_GPIOB, ENABLE);
		RCC_APB1PeriphClockCmd(RCC_APB1Periph_TIM12, ENABLE);
		portInit.GPIO_Pin   = GPIO_Pin_14 | GPIO_Pin_15;
		GPIO_Init(GPIOB, &portInit);
	}
}

void TimerInit(TIM_TypeDef* TIMx, uint16_t period, uint16_t pulse)
{
	TimerGPIOInit(TIMx);

	TIM_TimeBaseInitTypeDef timer;

	timer.TIM_Prescaler = (uint16_t) (SystemCoreClock / 1000000) - 1;
	timer.TIM_Period = period - 1;
	timer.TIM_ClockDivision = TIM_CKD_DIV1;
	timer.TIM_CounterMode = TIM_CounterMode_Up;
	timer.TIM_RepetitionCounter = 0;
	TIM_TimeBaseInit(TIMx, &timer);

	TimerOutputInit(TIMx, 0, pulse);
	TimerOutputInit(TIMx, 1, pulse);
	if (TIMx != TIM12)
	{
		TimerOutputInit(TIMx, 2, pulse);
		TimerOutputInit(TIMx, 3, pulse);
	}

	TIM_ARRPreloadConfig(TIMx, ENABLE);

	if (TIMx == TIM1 || TIMx == TIM8)
	{
		TIM_CtrlPWMOutputs(TIMx, ENABLE);
	}

	TIM_Cmd(TIMx, ENABLE);

	NVICInit(TIMx);
}

void TIM1_UP_TIM10_IRQHandler(void)
{
	if (TIM_GetITStatus(TIM1, TIM_IT_Update) != RESET)
	{
		TIM_ClearITPendingBit(TIM1, TIM_IT_Update);
		TIM_CCR4(TIM1, pwm_data[0][0], pwm_data[0][1], pwm_data[0][2], pwm_data[0][3]);
	}
}

void TIM2_IRQHandler(void)
{
	if (TIM_GetITStatus(TIM2, TIM_IT_Update) != RESET)
	{
		TIM_ClearITPendingBit(TIM2, TIM_IT_Update);
		TIM_CCR4(TIM2, pwm_data[1][0], pwm_data[1][1], pwm_data[1][2], pwm_data[1][3]);
	}
}

void TIM3_IRQHandler(void)
{
	if (TIM_GetITStatus(TIM3, TIM_IT_Update) != RESET)
	{
		TIM_ClearITPendingBit(TIM3, TIM_IT_Update);
		TIM_CCR4(TIM3, pwm_data[2][0], pwm_data[2][1], pwm_data[2][2], pwm_data[2][3]);
	}
}

void TIM4_IRQHandler(void)
{
	if (TIM_GetITStatus(TIM4, TIM_IT_Update) != RESET)
	{
		TIM_ClearITPendingBit(TIM4, TIM_IT_Update);
		TIM_CCR4(TIM4, pwm_data[3][0], pwm_data[3][1], pwm_data[3][2], pwm_data[3][3]);
	}
}

void TIM8_UP_TIM13_IRQHandler(void)
{
	if (TIM_GetITStatus(TIM8, TIM_IT_Update) != RESET)
	{
		TIM_ClearITPendingBit(TIM8, TIM_IT_Update);
		TIM_CCR4(TIM8, pwm_data[4][0], pwm_data[4][1], pwm_data[4][2], pwm_data[4][3]);
	}
}

void TIM8_BRK_TIM12_IRQHandler(void)
{
	if (TIM_GetITStatus(TIM12, TIM_IT_Update) != RESET)
	{
		TIM_ClearITPendingBit(TIM12, TIM_IT_Update);
		TIM_CCR2(TIM12, pwm_data[5][0], pwm_data[5][1]);
	}
}

void pwm_init(uint8_t index, uint16_t *data)
{
	pwm_data[index%6] = data;
	TimerInit(timers[index%6], PWM_PERIOD, PWM_MIN);
}

void pwm_enable(uint8_t index, uint8_t enable)
{
	TIM_Cmd(timers[index%6], enable ? ENABLE : DISABLE);
}

