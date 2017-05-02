using System.Text;

namespace ServoCommander
{
    public class HexModel
    {
        public byte LegsCount;
        public CoxaFemurTibia[] LegsAngle;
        public XYZ[] LegsPos;
        public ushort MoveTime;
        public ushort PrevMoveTime;

        public XYZ TotalTrans;
        public XYZ TotalBal;
        public XYZ BodyPos; // Body position
        public XYZ BodyRot; // X -Pitch, Y-Rotation, Z-Roll
        public int BodyYShift;
        public int BodyYOffset;
        public XYZ[] GatePos;
        public double[] GateRotY;
        public XYZ TravelLength;

        public ushort LegInitIndex;
        public ushort SelectedLeg;

        public bool PowerOn;
        public bool PrevPowerOn;

        public HexModel(byte legsCount)
        {
            LegsCount = legsCount;
            LegsAngle = new CoxaFemurTibia[legsCount];
            LegsPos = new XYZ[legsCount];

            TotalTrans = new XYZ();
            TotalBal = new XYZ();
            BodyPos = new XYZ();
            BodyRot = new XYZ();
            GatePos = new XYZ[legsCount];
            GateRotY = new double[legsCount];
            TravelLength = new XYZ();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            int i = 0;
            sb.AppendLine("LegsAngle:\n\r#,  Coxa,  Femur,  Tibia");
            foreach (var legAngle in LegsAngle) sb.AppendLine($"{i++} {legAngle}");
            i = 0;
            sb.AppendLine("LegsPos:\n\r#,   X,   Y,   Z");
            foreach (var v in LegsPos) sb.AppendLine($"{i++} {v} ");
            sb.AppendLine($"GateRotY: {string.Join(",", GateRotY)}");
            i = 0;
            sb.AppendLine("GatePos:\n\r#,   X,   Y,   Z");
            foreach (var v in GatePos) sb.AppendLine($"{i++} {v} ");
            sb.AppendLine($"TotalTrans: {TotalTrans} ");
            sb.AppendLine($"TotalBal: {TotalBal} ");
            sb.AppendLine($"BodyPos: {BodyPos} ");
            sb.AppendLine($"BodyRot: {BodyRot} ");
            sb.AppendLine($"SelectedLeg: {SelectedLeg,3}");
            sb.AppendLine($"MoveTime: {MoveTime}");
            sb.AppendLine($"PowerOn: {PowerOn,5}");
            return sb.ToString();
        }
    }
}
