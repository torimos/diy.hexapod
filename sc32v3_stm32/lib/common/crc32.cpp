#include  "crc32.h"

typedef struct
{
  __IO uint32_t DR;
  __IO uint8_t  IDR;
  uint8_t   RESERVED0;
  uint16_t  RESERVED1;
  __IO uint32_t CR;
} CRC_TypeDef;

#define CRC_CR_RESET ((uint8_t)0x01)
#define PERIPH_BASE ((uint32_t)0x40000000)
#define AHBPERIPH_BASE (PERIPH_BASE + 0x20000)
#define CRC_BASE (AHBPERIPH_BASE + 0x3000)
#define CRC ((CRC_TypeDef *) CRC_BASE)

uint32_t crc32(uint32_t *buffer, uint32_t size)
{
    rcc_clk_enable(RCC_CRC);
    CRC->CR = CRC_CR_RESET;
    for(uint32_t index = 0; index < size; index++)
    {
        CRC->DR = buffer[index];
    }
    return (CRC->DR);
}