namespace ServoCommander.Data
{
    public class HexConfig
    {
        public const int LegsCount = 6;
        public const double CoxaMin = -65;
        public const double CoxaMax = 65;
        public const double CoxaLength = 29;

        public const double FemurMin = -105;
        public const double FemurMax = 75;
        public const double FemurLength = 57;

        public const double TibiaMin = -53;
        public const double TibiaMax = 90;
        public const double TibiaLength = 141;

        public static short[] OffsetX = { -54, -108, -54, 54, 108, 54 }; //RR RM RF LF LM LR
        public static short[] OffsetZ = { 93, 0, -93, 93, 0, -93 }; //RR RM RF LF LM LR

        public static double[] CoxaDefaultAngle = { -59.7, 0, 59.7, -59.7, 0, 59.7 }; //RR RM RF LF LM LR

        public static double[] DefaultLegsPosX = { 56, 111, 56, 56, 111, 56 }; //RR RM RF LF LM LR
        public static double[] DefaultLegsPosY = { 65, 65, 65, 65, 65, 65 }; //RR RM RF LF LM LR
        public static double[] DefaultLegsPosZ = { 96, 0, -96, 96, 0, -96 }; //RR RM RF LF LM LR

        public static bool[] CoxaAngleInv = { true, true, true, false, false, false }; //RR RM RF LF LM LR
        public static bool[] FemurAngleInv = { true, true, true, false, false, false }; //RR RM RF LF LM LR
        public static bool[] TibiaAngleInv = { true, true, true, false, false, false }; //RR RM RF LF LM LR LF LM LR

        public const double MaxBodyHeight = 100;
        public const double BodyStandUpOffset = 45;
        public const double LegLiftHeight = 55;
        public const double LegLiftDoubleHeight = 80;
        public const double GPlimit = 2;
        public const double TravelDeadZone = 4;
    }
}
