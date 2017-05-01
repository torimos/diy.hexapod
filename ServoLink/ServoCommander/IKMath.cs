using System;

namespace ServoCommander
{
    public class IKMath
    {
        public enum IKSolutionResultType
        {
            Solution,
            Warning,
            Error
        }

        public struct IKResult
        {
            public double CoxaAngle;
            public double FemurAngle;
            public double TibiaAngle;
            public IKSolutionResultType Solution;
        }

        private double CheckBoundsAndSign(double value, double min, double max, bool inverted)
        {
            if (value < min) value = min;
            if (value > max) value = max;
            return inverted ? -value : value;
        }

        public IKResult LegIK(byte legNumber, double feetPosX, double feetPosY, double feetPosZ)
        {
            IKResult result = new IKResult();

            double IKSW;            //Length between Shoulder and Wrist
            double IKA1;            //Angle of the line S>W with respect to the ground in radians
            double IKA2;            //Angle of the line S>W with respect to the femur in radians
            result.CoxaAngle = CheckBoundsAndSign((Math.Atan2(feetPosZ, feetPosX) * 180) / Math.PI, IKMathConfig.CoxaMin, IKMathConfig.CoxaMax, IKMathConfig.CoxaAngleInv[legNumber]);
            double IKFeetPosXZFinal = Math.Sqrt(feetPosX * feetPosX + feetPosZ * feetPosZ) - IKMathConfig.CoxaLength;
            IKA1 = Math.Atan2(IKFeetPosXZFinal, feetPosY);
            IKSW = Math.Sqrt(feetPosY * feetPosY + IKFeetPosXZFinal * IKFeetPosXZFinal);
            IKA2 = Math.Acos(((IKMathConfig.FemurLength * IKMathConfig.FemurLength - IKMathConfig.TibiaLength * IKMathConfig.TibiaLength) + IKSW * IKSW) / (2 * IKMathConfig.FemurLength * IKSW));
            result.FemurAngle = CheckBoundsAndSign(- (IKA1 + IKA2) * 180 / Math.PI + 90, IKMathConfig.FemurMin, IKMathConfig.FemurMax, IKMathConfig.FemurAngleInv[legNumber]);
            double AngleRad4 = Math.Acos(((IKMathConfig.FemurLength * IKMathConfig.FemurLength + IKMathConfig.TibiaLength * IKMathConfig.TibiaLength) - IKSW * IKSW) / (2 * IKMathConfig.FemurLength * IKMathConfig.TibiaLength));
            result.TibiaAngle = CheckBoundsAndSign(-(90 - AngleRad4 * 180 / Math.PI), IKMathConfig.TibiaMin, IKMathConfig.TibiaMax, IKMathConfig.TibiaAngleInv[legNumber]);

            result.Solution = IKSolutionResultType.Error;
            if (IKSW < ((IKMathConfig.FemurLength + IKMathConfig.TibiaLength) - 30))
                result.Solution = IKSolutionResultType.Solution;
            else if (IKSW < (IKMathConfig.FemurLength + IKMathConfig.TibiaLength))
                result.Solution = IKSolutionResultType.Warning;
            return result;
        }
    }
}
