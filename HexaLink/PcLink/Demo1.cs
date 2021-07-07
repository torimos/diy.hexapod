using System;

namespace PcLink
{
    class Demo1
    {
        static SerialProtocol sp = new SerialProtocol();
       
        public static void Run(string[] args)
        {
            sp.Start();
            sp.OnFrameReady += OnFrameReady;

            //SendESP32ControllerSettings();

            while (!Console.KeyAvailable)
            {
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
