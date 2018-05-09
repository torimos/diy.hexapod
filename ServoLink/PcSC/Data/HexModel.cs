using System.Collections.Generic;
using System.Text;

namespace Data
{
    public enum GaitType
    {
        Ripple12,
        Tripod8,
        TripleTripod12,
        TripleTripod16,
        Wave24,
        Tripod6
    }

    public struct PhoenixGait
    {
        public short NomGaitSpeed;     //Nominal speed of the gait
        public byte StepsInGait;         //Number of steps in gait
        public byte NrLiftedPos;         //Number of positions that a single leg is lifted [1-3]
        public byte FrontDownPos;        //Where the leg should be put down to ground
        public byte LiftDivFactor;       //Normaly: 2, when NrLiftedPos=5: 4
        public byte TLDivFactor;         //Number of steps that a leg is on the floor while walking
        public byte HalfLiftHeight;      // How high to lift at halfway up.

        public byte[] GaitLegNr;       //Init position of the leg
    };


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

        public CoxaFemurTibia[] LegsAngle;
        public XYZ[] LegsPos;

        public XYZ TotalTrans; // Balanse Trans
        public XYZ TotalBal; // Balanse

        public XYZ BodyPos; // Body position
        public XYZ BodyRot; // X -Pitch, Y-Rotation, Z-Roll
        public double BodyYShift;
        public double BodyYOffset;

        public int LegInitIndex;
        public double LegsXZLength;
        public ushort SelectedLeg;
        public ushort PrevSelectedLeg;
        public XYZ SingleLegPos;
        public bool SingleLegHold;

        public ControlModeType ControlMode;
        public ControlModeType PrevControlMode;

        public ushort MoveTime;
        public ushort Speed;
        public ushort PrevMoveTime;
        public bool PowerOn;
        public bool PrevPowerOn;
        public int InputTimeDelay;

        public Dictionary<GaitType, PhoenixGait> Gaits;
        public GaitType GaitType;
        public byte GaitStep;
        public XYZ[] GaitPos;
        public double[] GaitRotY;
        public XYZ TravelLength;
        public bool BalanceMode;
        public PhoenixGait gaitCur;
        public byte ForceGaitStepCnt;
        public double LegLiftHeight;
        public bool Walking;
        public bool TravelRequest;
        public bool DoubleHeightOn;
        public bool DoubleTravelOn;
        public bool WalkMethod;
        public int ExtraCycle;

        public byte GPSeq;

        public bool DebugOutput;

        public long DebugDuration { get; internal set; }
        public int TimeToWait { get; internal set; }

        public HexModel(byte legsCount)
        {
            LegsAngle = new CoxaFemurTibia[legsCount];
            LegsPos = new XYZ[legsCount];

            TotalTrans = new XYZ();
            TotalBal = new XYZ();
            BodyPos = new XYZ();
            BodyRot = new XYZ();
            GaitPos = new XYZ[legsCount];
            GaitRotY = new double[legsCount];
            TravelLength = new XYZ();
            SingleLegPos = new XYZ();

            PrevControlMode = ControlMode = ControlModeType.Walk;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            int i = 0;
            sb.AppendLine("LegsAngle:\n\r#,  Coxa,  Femur,  Tibia");
            foreach (var legAngle in LegsAngle) sb.AppendLine($"{i} {legAngle} " + (i++== SelectedLeg ? "<<<<<" : "          "));

            sb.AppendLine();
            i = 0;
            sb.AppendLine("LegsPos:\n\r#,   X,   Y,   Z");
            foreach (var v in LegsPos) sb.AppendLine($"{i} {v} " + (i++ == SelectedLeg ? "<<<<<" : "          "));
            sb.AppendLine();
            sb.AppendLine($"SingleLegPos (Hold: {SingleLegHold,5}):\n\r     X,    Y,    Z\n\r{SingleLegPos}");

            sb.AppendLine();
            sb.AppendLine($"Body:\n\r     X,    Y,    Z,   RotX, RotY, RotZ,  YOffs,  YShift\n\r{BodyPos} {BodyRot} {BodyYOffset,5} {BodyYShift,5}");

            sb.AppendLine();
            sb.AppendLine($"TravelLength:\n\r     X,    Y,    Z\n\r{TravelLength}");
            i = 0;
            sb.AppendLine($"Gate [{GaitType,5}]:\n\r#,   X,     Y,     Z,     RotY");
            foreach (var v in GaitPos) { sb.AppendLine($"{i} {v} {GaitRotY[i].ToString("N1"),6} "); i++; };
            sb.AppendLine($"TravelRequest:{TravelRequest,5} Walking:{Walking,5} GaitStep:{GaitStep,2} ForceGaitStepCnt:{ForceGaitStepCnt,2} ExtraCycle:{ExtraCycle,2}");
            sb.AppendLine($"WalkMethod:{WalkMethod,5} DoubleHeightOn:{DoubleHeightOn,5} DoubleTravelOn:{DoubleTravelOn,5}");

            sb.AppendLine();
            sb.AppendLine($"BalanceMode: {BalanceMode,5} ");
            sb.AppendLine($"TotalTrans: {TotalTrans} ");
            sb.AppendLine($"TotalBal: {TotalBal} ");

            sb.AppendLine();
            sb.AppendLine($"Speed: {Speed, 5}");
            sb.AppendLine($"MoveTime: {MoveTime,5}");
            sb.AppendLine($"InputTimeDelay: {InputTimeDelay,5}");
            sb.AppendLine($"ControlMode: {ControlMode,10}");
            sb.AppendLine($"PowerOn: {PowerOn,5}");
            return sb.ToString();
        }
    }
}
