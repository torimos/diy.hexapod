using System;
using System.IO;

namespace PcLink
{
    class Program
    {
        static SerialProtocol sp = new SerialProtocol();
        static byte[] GetBinaryArray(uint[] servos)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            foreach (var servo in servos) bw.Write(servo);
            return ms.ToArray();
        }
        static void Main(string[] args)
        {
            sp.Create();
            sp.OnFrameReady += OnFrameReady;

            //SendESP32ControllerSettings();

            uint[] servos = new uint[26];
            servos[0] = 0xDEADBEAF;
            while (!Console.KeyAvailable)
            {
                sp.Loop();
                sp.SendFrame(FrameHeaderType.STM32Debug, GetBinaryArray(servos));
                servos[1]++;
            }
        }

        private static void SendESP32ControllerSettings()
        {
            var settings = Settings.Load("hexapod.settings.json");
            //new Settings().Save("hexapod.settings2.json");
            var fs = new FrameSettingsData
            {
                settings = settings,
                save = false
            };
            sp.SendFrame(FrameHeaderType.ESP32Debug, fs.ToArray());
        }

        private static void OnFrameReady(object sender, FrameReadyEventArgs e)
        {
            Console.Write($"FPS: {sp.FPS}. CPS: {e.Model.cps} Power: {e.Model.turnedOn}. [{e.Servos.Length}]: ");
            for (int i = 0; i < e.Servos.Length; i++)
                Console.Write($"{e.Servos[i]:X} ");
            Console.WriteLine();
        }

    }
}
