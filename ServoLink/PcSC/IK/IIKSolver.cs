using Data;

namespace IK
{
    public interface IIKSolver
    {
        IKLegResult LegIK(byte legNumber, double feetPosX, double feetPosZ, double feetPosY);
        XYZ BodyFK(byte legNumber, double PosX, double PosZ, double PosY, double RotationY, double BodyRotX, double BodyRotZ, double BodyRotY, double TotalXBal, double TotalZBal, double TotalYBal);
    }
}
