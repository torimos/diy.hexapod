#include  "crc32.h"

u32 crc32(uint32_t *buffer, u32 size)
{
	RCC_AHBPeriphClockCmd(RCC_AHBPeriph_CRC, ENABLE);
	CRC_ResetDR();
	return CRC_CalcBlockCRC(buffer, size);
}

u32 crc32w(u32 v)
{
	RCC_AHBPeriphClockCmd(RCC_AHBPeriph_CRC, ENABLE);
	//CRC_ResetDR();
	//return CRC_CalcCRC(v);
	CRC->CR = 1;
	CRC->DR = v;
	return CRC->DR;
}