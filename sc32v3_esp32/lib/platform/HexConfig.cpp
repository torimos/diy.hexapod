#include "HexConfig.h"

int HexConfig::LegsCount = 6;
double HexConfig::CoxaMin = -65;//-95
double HexConfig::CoxaMax = 65;//75
double HexConfig::CoxaLength = 30;

double HexConfig::FemurMin = -105;
double HexConfig::FemurMax = 45;
double HexConfig::FemurLength = 55;

double HexConfig::TibiaMin = -40;
double HexConfig::TibiaMax = 80;
double HexConfig::TibiaLength = 140;

short HexConfig::OffsetX[] = { -54, -108, -54, 54, 108, 54 }; //RR RM RF LF LM LR

short HexConfig::OffsetZ[] = { 93, 0, -93, 93, 0, -93 }; //RR RM RF LF LM LR

double HexConfig::CoxaDefaultAngle[] = { -59.7, 0, 59.7, -59.7, 0, 59.7 }; //RR RM RF LF LM LR

double HexConfig::DefaultLegsPosX[] = { 56, 111, 56, 56, 111, 56 }; //RR RM RF LF LM LR
double HexConfig::DefaultLegsPosY[] = { 65, 65, 65, 65, 65, 65 }; //RR RM RF LF LM LR
double HexConfig::DefaultLegsPosZ[] = { 96, 0, -96, 96, 0, -96 }; //RR RM RF LF LM LR

bool HexConfig::CoxaAngleInv[] = { true, true, true, false, false, false }; //RR RM RF LF LM LR
bool HexConfig::FemurAngleInv[] = { true, true, true, false, false, false }; //RR RM RF LF LM LR
bool HexConfig::TibiaAngleInv[] = { true, true, true, false, false, false }; //RR RM RF LF LM LR LF LM LR

double HexConfig::MaxBodyHeight = 110;
double HexConfig::BodyStandUpOffset = 55;
double HexConfig::LegLiftHeight = 40;
double HexConfig::LegLiftDoubleHeight = 65;
double HexConfig::GPlimit = 2;
double HexConfig::TravelDeadZone = 4;

uint16_t HexConfig::WalkingDelay =  200;
uint16_t HexConfig::BalancingDelay = 100;
uint16_t HexConfig::SingleLegControlDelay = 25;

int HexConfig::GaitsCount = 6;