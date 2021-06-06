This is Hexapod project.

Project has different versions of hexapod firmwares: 
- v1: is VisugalGDB based project for Visual Studio. Servo controller was based on stm32 custom board with multiple external connections (1xSPI, 1xI2C, 2xUART) for controll and communication. Used with /ServoLink/PcSC to controll board using uart-ble adapter 
- v2: sc32v2 is PlatformIO based project written for same pcb board as in v1. Used with /ServoLink/PcSC to controll board using uart-ble adapter
- v2: sc32v3 is new PlatformIO based projects written for beand new pcb design based on stm32 as servo controller and esp32 as ble/wifi adapter + set of additional ports for extenral communications. Key difference of this project it does not require remote pc to controll hexapod. ESP32 is used as host.


This project is inspired by https://github.com/KurtE/Arduino_Phoenix_Parts
