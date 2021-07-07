#include "HexConfig.h"

int HexConfig::LegsCount = 6;
double HexConfig::MaxBodyHeight = 90;
double HexConfig::BodyInitY  = 65;

double HexConfig::CoxaMin = -65;
double HexConfig::CoxaMax = 65;
double HexConfig::CoxaLength = 29;
double HexConfig::FemurMin = -105;
double HexConfig::FemurMax = 75;
double HexConfig::FemurLength = 57;
double HexConfig::TibiaMin = -53;
double HexConfig::TibiaMax = 90;
double HexConfig::TibiaLength = 141;
short HexConfig::OffsetX[] = { -69, -138, -69, 69, 138, 69 }; //RR RM RF LF LM LR
short HexConfig::OffsetZ[] = { 119, 0, -119, 119, 0, -119 }; //RR RM RF LF LM LR
double HexConfig::CoxaDefaultAngle[] = { -60, 0, 60, -60, 0, 60 }; //RR RM RF LF LM LR

double HexConfig::DefaultLegsPosX[] = { 56, 111, 56, 56, 111, 56 }; //RR RM RF LF LM LR
double HexConfig::DefaultLegsPosY[] = { HexConfig::BodyInitY, HexConfig::BodyInitY, HexConfig::BodyInitY, HexConfig::BodyInitY, HexConfig::BodyInitY, HexConfig::BodyInitY }; //RR RM RF LF LM LR
double HexConfig::DefaultLegsPosZ[] = { 96, 0, -96, 96, 0, -96 }; //RR RM RF LF LM LR

int HexConfig::HexIntXZCount = 3;
double HexConfig::HexIntXZ[] = { 111, 99, 86 };
double HexConfig::HexMaxBodyY[] = { 20, 50, HexConfig::MaxBodyHeight };

bool HexConfig::CoxaAngleInv[] = { true, true, true, false, false, false }; //RR RM RF LF LM LR
bool HexConfig::FemurAngleInv[] = { true, true, true, false, false, false }; //RR RM RF LF LM LR
bool HexConfig::TibiaAngleInv[] = { true, true, true, false, false, false }; //RR RM RF LF LM LR LF LM LR

double HexConfig::BodyStandUpOffset = HexConfig::BodyInitY;
double HexConfig::LegLiftHeight = 40;
double HexConfig::LegLiftDoubleHeight = HexConfig::BodyInitY;
double HexConfig::GPlimit = 2;
double HexConfig::TravelDeadZone = 4;

uint16_t HexConfig::WalkingDelay =  200;
uint16_t HexConfig::BalancingDelay = 100;
uint16_t HexConfig::SingleLegControlDelay = 25;

int HexConfig::GaitsCount = 6;