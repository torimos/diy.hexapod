#include "IKSolver.h"
#include <math.h>

IKSolver::IKSolver()
{
}


IKSolver::~IKSolver()
{
}

double CheckBoundsAndSign(double value, double min, double max, bool inverted)
{
	if (value < min) value = min;
	if (value > max) value = max;
	return inverted ? -value : value;
}

double acos_truncated(double a)
{
	double v = a;
	if (v < -1) v = -1;
	else if (v > 1) v = 1;
	return acos(v);
}
/*

IKLegResult result = IKLegResult();
	double IKSW;            
	double IKA1;            
	double IKA2;            
	double IKA3;
	// Distance between the Coxa and Ground Contact 
	double IKFeetPosXZFinal = sqrt(pow(feetPosX,2) + pow(feetPosZ,2));
	// Length between Femur axis and Tibia
	IKSW = sqrt(pow(feetPosY,feetPosY) + pow(IKFeetPosXZFinal - HexConfig::CoxaLength,2));
	// Angle between Femur and Tibia line and the ground in radians 
	IKA1 = atan2(IKFeetPosXZFinal - HexConfig::CoxaLength, feetPosY);
	// Angle of the line Femur and Tibia with respect to the Femur in radians 
	IKA2 = acos_truncated(((pow(HexConfig::FemurLength,2) - pow(HexConfig::TibiaLength,2)) + IKSW * IKSW) / (2 * HexConfig::FemurLength * IKSW));
	IKA3 = acos_truncated(((pow(HexConfig::FemurLength,2) + pow(HexConfig::TibiaLength,2)) - IKSW * IKSW) / (2 * HexConfig::FemurLength * HexConfig::TibiaLength));
	
	result.Result.Coxa = CheckBoundsAndSign(atan2(feetPosZ, feetPosX) * 180 / M_PI + HexConfig::CoxaDefaultAngle[legNumber], HexConfig::CoxaMin, HexConfig::CoxaMax, HexConfig::CoxaAngleInv[legNumber]);
	result.Result.Femur = CheckBoundsAndSign(-(IKA1 + IKA2) * 180 / M_PI + HexConfig::FemurDefaultAngle, HexConfig::FemurMin, HexConfig::FemurMax, HexConfig::FemurAngleInv[legNumber]);
	result.Result.Tibia = CheckBoundsAndSign(IKA3 * 180 / M_PI + HexConfig::TibiaDefaultAngle, HexConfig::TibiaMin, HexConfig::TibiaMax, HexConfig::TibiaAngleInv[legNumber]);

	if (isnan(result.Result.Tibia) || isnan(result.Result.Femur) || isnan(result.Result.Coxa))
	{
		Log.printf("LegIK - EXCEPTION. leg:%d t:%f f:%f c:%f fx:%f fy:%f fz:%f\n\r", legNumber, result.Result.Tibia, result.Result.Femur, result.Result.Coxa, feetPosX, feetPosY, feetPosZ);
		while(1){
  			StateLed.Flash(CRGB(8,8,0), 2, 500);
			delay(100);
		}
	}

	result.Solution = IKSolutionResultType::Error;
	if (IKSW < ((HexConfig::FemurLength + HexConfig::TibiaLength) - HexConfig::CoxaLength))
		result.Solution = IKSolutionResultType::Solution;
	else if (IKSW < (HexConfig::FemurLength + HexConfig::TibiaLength))
		result.Solution = IKSolutionResultType::Warning;
	return result;
*/
IKLegResult IKSolver::LegIK(uint8_t legNumber, double feetPosX, double feetPosZ, double feetPosY)
{
	IKLegResult result = IKLegResult();
	double IKSW;            //Length between Shoulder and Wrist
	double IKA1;            //Angle of the line S>W with respect to the ground in radians
	double IKA2;            //Angle of the line S>W with respect to the femur in radians
	double IKA3;
	double IKFeetPosXZFinal = sqrt(feetPosX * feetPosX + feetPosZ * feetPosZ) - HexConfig::CoxaLength;
	
	IKSW = sqrt(feetPosY * feetPosY + IKFeetPosXZFinal * IKFeetPosXZFinal);
	IKA1 = atan2(IKFeetPosXZFinal, feetPosY);
	IKA2 = acos_truncated(((HexConfig::FemurLength * HexConfig::FemurLength - HexConfig::TibiaLength * HexConfig::TibiaLength) + IKSW * IKSW) / (2 * HexConfig::FemurLength * IKSW));
	IKA3 = acos_truncated(((HexConfig::FemurLength * HexConfig::FemurLength + HexConfig::TibiaLength * HexConfig::TibiaLength) - IKSW * IKSW) / (2 * HexConfig::FemurLength * HexConfig::TibiaLength));
	
	result.Result.Coxa = CheckBoundsAndSign(((atan2(feetPosZ, feetPosX) * 180) / M_PI) + HexConfig::CoxaDefaultAngle[legNumber], HexConfig::CoxaMin, HexConfig::CoxaMax, HexConfig::CoxaAngleInv[legNumber]);
	result.Result.Femur = CheckBoundsAndSign(-(IKA1 + IKA2) * 180 / M_PI + 90, HexConfig::FemurMin, HexConfig::FemurMax, HexConfig::FemurAngleInv[legNumber]);
	result.Result.Tibia = CheckBoundsAndSign(IKA3 * 180 / M_PI - 90, HexConfig::TibiaMin, HexConfig::TibiaMax, HexConfig::TibiaAngleInv[legNumber]);

	if (isnan(result.Result.Tibia) || isnan(result.Result.Femur) || isnan(result.Result.Coxa))
	{
		Log.printf("LegIK - EXCEPTION. leg:%d t:%f f:%f c:%f fx:%f fy:%f fz:%f\n\r", legNumber, result.Result.Tibia, result.Result.Femur, result.Result.Coxa, feetPosX, feetPosY, feetPosZ);
		while(1){
  			StateLed.Flash(CRGB(8,8,0), 2, 500);
			delay(100);
		}
	}

	result.Solution = IKSolutionResultType::Error;
	if (IKSW < ((HexConfig::FemurLength + HexConfig::TibiaLength) - HexConfig::CoxaLength))
		result.Solution = IKSolutionResultType::Solution;
	else if (IKSW < (HexConfig::FemurLength + HexConfig::TibiaLength))
		result.Solution = IKSolutionResultType::Warning;
	return result;
}

XYZ IKSolver::BodyFK(uint8_t legNumber, double PosX, double PosZ, double PosY, double RotationY, double BodyRotX, double BodyRotZ, double BodyRotY, double TotalXBal, double TotalZBal, double TotalYBal)
{
	double SinA;			//Sin buffer for BodyRotX calculations
	double CosA;			//Cos buffer for BodyRotX calculations
	double SinB;			//Sin buffer for BodyRotX calculations
	double CosB;			//Cos buffer for BodyRotX calculations
	double SinG;			//Sin buffer for BodyRotZ calculations
	double CosG;			//Cos buffer for BodyRotZ calculations
	double CPR_X;			//Final X value for centerpoint of rotation
	double CPR_Y;           //Final Y value for centerpoint of rotation
	double CPR_Z;           //Final Z value for centerpoint of rotation
	double c1DEC = 10;

	//Calculating totals from center of the body to the feet 
	CPR_X = HexConfig::OffsetX[legNumber] + PosX;
	CPR_Y = PosY; //Define centerpoint for rotation along the Y-axis
	CPR_Z = HexConfig::OffsetZ[legNumber] + PosZ;

	//Successive global rotation matrix: 
	//Math shorts for rotation: Alfa [A] = Xrotate, Beta [B] = Zrotate, Gamma [G] = Yrotate 
	//Sinus Alfa = SinA, cosinus Alfa = cosA. and so on... 

	//First calculate sinus and cosinus for each rotation: 
	double bx = M_PI * ((BodyRotX + TotalXBal) / c1DEC) / 180.0;
	SinG = sin(bx);
	CosG = cos(bx);

	double bz = M_PI * ((BodyRotZ + TotalZBal) / c1DEC) / 180.0;
	SinB = sin(bz);
	CosB = cos(bz);

	double by = M_PI * ((BodyRotY + (RotationY * c1DEC) + TotalYBal) / c1DEC) / 180.0;
	SinA = sin(by);
	CosA = cos(by);

	//Calcualtion of rotation matrix: 
	double BodyFKPosX = (CPR_X - (CPR_X * CosA * CosB - CPR_Z * CosB * SinA + CPR_Y * SinB));
	double BodyFKPosZ = (CPR_Z - (CPR_X * CosG * SinA + CPR_X * CosA * SinB * SinG + CPR_Z * CosA * CosG - CPR_Z * SinA * SinB * SinG - CPR_Y * CosB * SinG));
	double BodyFKPosY = (CPR_Y - (CPR_X * SinA * SinG - CPR_X * CosA * CosG * SinB + CPR_Z * CosA * SinG + CPR_Z * CosG * SinA * SinB + CPR_Y * CosB * CosG));

	return { BodyFKPosX, BodyFKPosY, BodyFKPosZ };
}