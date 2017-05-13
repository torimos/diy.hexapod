using System;

namespace ServoCommander
{
    public partial class IKSolver: IIKSolver
    {
        IKMath math = new IKMath();

        public IKLegResult LegIK(byte legNumber, double feetPosX, double feetPosZ, double feetPosY)
        {
            IKLegResult result = new IKLegResult();
            long IKSW;            //Length between Shoulder and Wrist
            long IKA1;            //Angle of the line S>W with respect to the ground in radians
            long IKA2;            //Angle of the line S>W with respect to the femur in radians

            math.GetATan2((long)feetPosX, (long)feetPosZ);
            long coxa = (long)(((math.Atan4 * 180) / IKMath.PI) + HexConfig.CoxaDefaultAngle[legNumber]*IKMath.c1DEC);

            long IKFeetPosXZ = math.XYhyp2 / IKMath.c2DEC;
            long IKA14 = math.GetATan2((long)feetPosY, IKFeetPosXZ - (long)HexConfig.CoxaLength);
            long IKSW2 = math.XYhyp2;

            long Temp1 = ((long)(HexConfig.FemurLength * HexConfig.FemurLength - HexConfig.TibiaLength * HexConfig.TibiaLength) * IKMath.c4DEC + ((long)IKSW2 * IKSW2));
            long Temp2 = (long)(2 * HexConfig.FemurLength) * IKMath.c2DEC * IKSW2;
            long T3 = Temp1 / (Temp2 / IKMath.c4DEC);
            long IKA24 = math.GetArcCos(T3);
            long femur = -(long)(IKA14 + IKA24) * 180 / IKMath.PI + 900;

            Temp1 = (long)(HexConfig.FemurLength * HexConfig.FemurLength + HexConfig.TibiaLength * HexConfig.TibiaLength) * IKMath.c4DEC - IKSW2 * IKSW2;
            Temp2 = (long)(2 * HexConfig.FemurLength * HexConfig.TibiaLength);
            math.GetArcCos(Temp1 / Temp2);
            long tibia = -(900 - math.AngleRad4 * 180 / IKMath.PI);

            result.Result.Coxa = IKMath.CheckBoundsAndSign(coxa / 10, HexConfig.CoxaMin, HexConfig.CoxaMax, HexConfig.CoxaAngleInv[legNumber]);
            result.Result.Femur = IKMath.CheckBoundsAndSign(femur / 10, HexConfig.FemurMin, HexConfig.FemurMax, HexConfig.FemurAngleInv[legNumber]);
            result.Result.Tibia = IKMath.CheckBoundsAndSign(tibia / 10, HexConfig.TibiaMin, HexConfig.TibiaMax, HexConfig.TibiaAngleInv[legNumber]);
            result.Solution = IKSolutionResultType.Error;
            if (IKSW2 < ((HexConfig.FemurLength + HexConfig.TibiaLength) - 30) * IKMath.c2DEC)
                result.Solution = IKSolutionResultType.Solution;
            else if (IKSW2 < (HexConfig.FemurLength + HexConfig.TibiaLength) * IKMath.c2DEC)
                result.Solution = IKSolutionResultType.Warning;
            return result;
        }

        public XYZ BodyFK(byte legNumber, double PosX, double PosZ, double PosY, double RotationY, double BodyRotX, double BodyRotZ, double BodyRotY, double TotalXBal, double TotalZBal, double TotalYBal)
        {
            long SinA4;          //Sin buffer for BodyRotX calculations
            long CosA4;          //Cos buffer for BodyRotX calculations
            long SinB4;          //Sin buffer for BodyRotX calculations
            long CosB4;          //Cos buffer for BodyRotX calculations
            long SinG4;          //Sin buffer for BodyRotZ calculations
            long CosG4;          //Cos buffer for BodyRotZ calculations
            long CPR_X;            //Final X value for centerpoint of rotation
            long CPR_Y;            //Final Y value for centerpoint of rotation
            long CPR_Z;            //Final Z value for centerpoint of rotation

            //Calculating totals from center of the body to the feet 
            CPR_X = (long)(HexConfig.OffsetX[legNumber] + PosX);
            CPR_Y = (long)PosY; //Define centerpoint for rotation along the Y-axis
            CPR_Z = (long)(HexConfig.OffsetZ[legNumber] + PosZ);

            //Successive global rotation matrix: 
            //Math shorts for rotation: Alfa [A] = Xrotate, Beta [B] = Zrotate, Gamma [G] = Yrotate 
            //Sinus Alfa = SinA, cosinus Alfa = cosA. and so on... 

            //First calculate sinus and cosinus for each rotation: 
            math.GetSinCos((long)(BodyRotX + TotalXBal));
            SinG4 = math.sin4;
            CosG4 = math.cos4;

            math.GetSinCos((long)(BodyRotZ + TotalZBal));
            SinB4 = math.sin4;
            CosB4 = math.cos4;

            math.GetSinCos((long)(BodyRotY + (RotationY * IKMath.c1DEC) + TotalYBal));
            SinA4 = math.sin4;
            CosA4 = math.cos4;

            //Calcualtion of rotation matrix: 
            long BodyFKPosX = ((long)CPR_X * IKMath.c2DEC - ((long)CPR_X * IKMath.c2DEC * CosA4 / IKMath.c4DEC * CosB4 / IKMath.c4DEC - (long)CPR_Z * IKMath.c2DEC * CosB4 / IKMath.c4DEC * SinA4 / IKMath.c4DEC
                + (long)CPR_Y * IKMath.c2DEC * SinB4 / IKMath.c4DEC)) / IKMath.c2DEC;
            long BodyFKPosZ = ((long)CPR_Z * IKMath.c2DEC - ((long)CPR_X * IKMath.c2DEC * CosG4 / IKMath.c4DEC * SinA4 / IKMath.c4DEC + (long)CPR_X * IKMath.c2DEC * CosA4 / IKMath.c4DEC * SinB4 / IKMath.c4DEC * SinG4 / IKMath.c4DEC
                + (long)CPR_Z * IKMath.c2DEC * CosA4 / IKMath.c4DEC * CosG4 / IKMath.c4DEC - (long)CPR_Z * IKMath.c2DEC * SinA4 / IKMath.c4DEC * SinB4 / IKMath.c4DEC * SinG4 / IKMath.c4DEC
                - (long)CPR_Y * IKMath.c2DEC * CosB4 / IKMath.c4DEC * SinG4 / IKMath.c4DEC)) / IKMath.c2DEC;
            long BodyFKPosY = ((long)CPR_Y * IKMath.c2DEC - ((long)CPR_X * IKMath.c2DEC * SinA4 / IKMath.c4DEC * SinG4 / IKMath.c4DEC - (long)CPR_X * IKMath.c2DEC * CosA4 / IKMath.c4DEC * CosG4 / IKMath.c4DEC * SinB4 / IKMath.c4DEC
                + (long)CPR_Z * IKMath.c2DEC * CosA4 / IKMath.c4DEC * SinG4 / IKMath.c4DEC + (long)CPR_Z * IKMath.c2DEC * CosG4 / IKMath.c4DEC * SinA4 / IKMath.c4DEC * SinB4 / IKMath.c4DEC
                + (long)CPR_Y * IKMath.c2DEC * CosB4 / IKMath.c4DEC * CosG4 / IKMath.c4DEC)) / IKMath.c2DEC;
            return new XYZ(BodyFKPosX, BodyFKPosY, BodyFKPosZ);
        }
    }
}
