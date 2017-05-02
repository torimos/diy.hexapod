namespace ServoCommander
{
    partial class Program
    {
        struct HexModel
        {
            public IKLegResult[] LegsAngle;
            public XYZ[] LegsPos;
            public ushort MoveTime;
            public byte LegsCount;

            public HexModel(byte legsCount)
            {
                LegsCount = legsCount;
                LegsAngle = new IKLegResult[legsCount];
                LegsPos = new XYZ[legsCount];
                MoveTime = 100;
            }
        }
    }
}
