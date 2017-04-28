using System;

namespace ServoLink.Contracts
{
    public interface IServoController
    {
        bool Connect(IPort port);
        void Disconnect();
        int Commit();
        void MoveAll(ushort position, ushort time);
        void Move(int index, ushort position, ushort time);
    }
}
