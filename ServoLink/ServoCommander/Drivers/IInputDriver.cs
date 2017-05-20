using ServoCommander.Data;

namespace ServoCommander.Drivers
{
    public interface IInputDriver
    {
        bool Terminate { get; set; }

        void ProcessInput(HexModel model);

        void Release();

        void DebugOutput();
    }
}