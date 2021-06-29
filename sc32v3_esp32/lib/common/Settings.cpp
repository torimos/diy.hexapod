#include "Settings.h"
#include <stdint.h>
#include "EEPROM.h"

void settings_init()
{
    static bool _eeprom_initialized = false;
    if (!_eeprom_initialized)
    {
        EEPROM.begin(1024);
    }
}

void settings_read(int pos, void* data, int len)
{
    settings_init();
    int i = 0;
    while(i<len)
    {
        *((uint8_t*)((int)data+i)) = EEPROM.read(pos+i);
        i++;
    }
}

void settings_write(int pos, void* data, int len)
{
    settings_init();
    int i = 0;
    while(i<len)
    {
        EEPROM.write(pos+i, *((uint8_t*)((int)data+i)));
        i++;
    }

    EEPROM.commit();
}