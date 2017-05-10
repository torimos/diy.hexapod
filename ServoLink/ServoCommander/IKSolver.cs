using System;

namespace ServoCommander
{
    public partial class IKSolver
    {
        private double CheckBoundsAndSign(double value, double min, double max, bool inverted)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return inverted ? -value : value;
        }

        public IKLegResult LegIK(byte legNumber, double feetPosX, double feetPosZ, double feetPosY)
        {
            IKLegResult result = new IKLegResult();
            double IKSW;            //Length between Shoulder and Wrist
            double IKA1;            //Angle of the line S>W with respect to the ground in radians
            double IKA2;            //Angle of the line S>W with respect to the femur in radians
            result.Result.Coxa = CheckBoundsAndSign(((Math.Atan2(feetPosZ, feetPosX) * 180) / Math.PI) + HexConfig.CoxaDefaultAngle[legNumber], HexConfig.CoxaMin, HexConfig.CoxaMax, HexConfig.CoxaAngleInv[legNumber]);
            double IKFeetPosXZFinal = Math.Sqrt(feetPosX * feetPosX + feetPosZ * feetPosZ) - HexConfig.CoxaLength;
            IKA1 = Math.Atan2(IKFeetPosXZFinal, feetPosY);
            IKSW = Math.Sqrt(feetPosY * feetPosY + IKFeetPosXZFinal * IKFeetPosXZFinal);
            IKA2 = Math.Acos(((HexConfig.FemurLength * HexConfig.FemurLength - HexConfig.TibiaLength * HexConfig.TibiaLength) + IKSW * IKSW) / (2 * HexConfig.FemurLength * IKSW));
            result.Result.Femur = CheckBoundsAndSign(- (IKA1 + IKA2) * 180 / Math.PI + 90, HexConfig.FemurMin, HexConfig.FemurMax, HexConfig.FemurAngleInv[legNumber]);
            double AngleRad4 = Math.Acos(((HexConfig.FemurLength * HexConfig.FemurLength + HexConfig.TibiaLength * HexConfig.TibiaLength) - IKSW * IKSW) / (2 * HexConfig.FemurLength * HexConfig.TibiaLength));
            result.Result.Tibia = CheckBoundsAndSign(-(90 - AngleRad4 * 180 / Math.PI), HexConfig.TibiaMin, HexConfig.TibiaMax, HexConfig.TibiaAngleInv[legNumber]);

            result.Solution = IKSolutionResultType.Error;
            if (IKSW < ((HexConfig.FemurLength + HexConfig.TibiaLength) - 30))
                result.Solution = IKSolutionResultType.Solution;
            else if (IKSW < (HexConfig.FemurLength + HexConfig.TibiaLength))
                result.Solution = IKSolutionResultType.Warning;
            return result;
        }

        public XYZ BodyFK(byte legNumber, double PosX, double PosZ, double PosY, double RotationY, double BodyRotX, double BodyRotZ, double BodyRotY, double TotalXBal, double TotalZBal, double TotalYBal)
        {
            double SinA;          //Sin buffer for BodyRotX calculations
            double CosA;          //Cos buffer for BodyRotX calculations
            double SinB;          //Sin buffer for BodyRotX calculations
            double CosB;          //Cos buffer for BodyRotX calculations
            double SinG;          //Sin buffer for BodyRotZ calculations
            double CosG;          //Cos buffer for BodyRotZ calculations
            double CPR_X;            //Final X value for centerpoint of rotation
            double CPR_Y;            //Final Y value for centerpoint of rotation
            double CPR_Z;            //Final Z value for centerpoint of rotation
            double c1DEC = 10;

            //Calculating totals from center of the body to the feet 
            CPR_X = HexConfig.OffsetX[legNumber] + PosX;
            CPR_Y = PosY; //Define centerpoint for rotation along the Y-axis
            CPR_Z = HexConfig.OffsetZ[legNumber] + PosZ;

            //Successive global rotation matrix: 
            //Math shorts for rotation: Alfa [A] = Xrotate, Beta [B] = Zrotate, Gamma [G] = Yrotate 
            //Sinus Alfa = SinA, cosinus Alfa = cosA. and so on... 

            //First calculate sinus and cosinus for each rotation: 

            double bx = Math.PI * ((BodyRotX + TotalXBal) / c1DEC) / 180.0;
            SinG = Math.Sin(bx);
            CosG = Math.Cos(bx);

            double bz = Math.PI * ((BodyRotZ + TotalZBal) / c1DEC) / 180.0;
            SinB = Math.Sin(bz);
            CosB = Math.Cos(bz);

            double by = Math.PI * ((BodyRotY + (RotationY * c1DEC) + TotalYBal) / c1DEC) / 180.0;
            SinA = Math.Sin(by);
            CosA = Math.Cos(by);

            //Calcualtion of rotation matrix: 
            double BodyFKPosX = (CPR_X - (CPR_X * CosA * CosB - CPR_Z * CosB * SinA + CPR_Y * SinB));
            double BodyFKPosZ = (CPR_Z - (CPR_X * CosG * SinA + CPR_X * CosA * SinB * SinG + CPR_Z * CosA * CosG - CPR_Z * SinA * SinB * SinG - CPR_Y * CosB * SinG));
            double BodyFKPosY = (CPR_Y - (CPR_X * SinA * SinG - CPR_X * CosA * CosG * SinB + CPR_Z * CosA * SinG + CPR_Z * CosG * SinA * SinB + CPR_Y * CosB * CosG));

            return new XYZ(BodyFKPosX, BodyFKPosY, BodyFKPosZ);
        }

    }
}
