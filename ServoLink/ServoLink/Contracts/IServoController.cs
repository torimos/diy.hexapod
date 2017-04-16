using System;

namespace ServoLink.Contracts
{
    public interface IServoController
    {
        bool Connect(IPort port);
        void Disconnect();
        void Sync();
        void SetAll(ushort position);
    }
}
