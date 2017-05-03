using System.Text;

namespace ServoCommander
{
    public class HexModel
    {
        public enum ControlModeType
        {
            Walk = 0,
            Translate,
            Rotate,
            SingleLeg,
            GPPlayer
        }

        public byte LegsCount;
        public CoxaFemurTibia[] LegsAngle;
        public XYZ[] LegsPos;
        public ushort MoveTime;
        public ushort PrevMoveTime;

        public XYZ TotalTrans;
        public XYZ TotalBal;
        public XYZ BodyPos; // Body position
        public XYZ BodyRot; // X -Pitch, Y-Rotation, Z-Roll
        public double BodyYShift;
        public double BodyYOffset;
        public XYZ[] GatePos;
        public double[] GateRotY;
        public XYZ TravelLength;

        public ushort LegInitIndex;
        public ushort SelectedLeg;

        public ControlModeType ControlMode;

        public bool PowerOn;
        public bool PrevPowerOn;
        internal bool AdjustLegsPosition;
        internal int InputTimeDelay;

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

            ControlMode = ControlModeType.Translate;
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
            sb.AppendLine($"BodyYOffset: {BodyYOffset,5}");
            sb.AppendLine($"BodyYShift: {BodyYShift,5}");
            sb.AppendLine($"SelectedLeg: {SelectedLeg,3}");
            sb.AppendLine($"MoveTime: {MoveTime, 4}");
            sb.AppendLine($"ControlMode: {ControlMode,10}");
            sb.AppendLine($"InputTimeDelay: {InputTimeDelay,5}");
            sb.AppendLine($"PowerOn: {PowerOn,5}");
            return sb.ToString();
        }
    }
}
