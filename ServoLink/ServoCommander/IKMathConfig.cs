namespace ServoCommander
{
    public class IKMathConfig
    {
        public const double CoxaMin = -65;
        public const double CoxaMax = 65;
        public const double CoxaLength = 29;

        public const double FemurMin = -105;
        public const double FemurMax = 75;
        public const double FemurLength = 57;

        public const double TibiaMin = -53;
        public const double TibiaMax = 90;
        public const double TibiaLength = 141;
        public const uint LegsCount = 6;

        public static bool[] CoxaAngleInv = { false, true, false, false, false, false }; //RR RM RF LF LM LR
        public static bool[] FemurAngleInv = { true, true, true, false, false, false }; //RR RM RF LF LM LR
        public static bool[] TibiaAngleInv = { true, true, true, false, false, false }; //RR RM RF LF LM LR LF LM LR
    }
}
