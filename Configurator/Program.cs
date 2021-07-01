using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
namespace TestLine
{
    public class Settings
    {
        public short[] ServoOffset;
        public short[] ServoInv;
        public short[] ServoMap;
        public Settings()
        {
            // RR,RM,RF,LR,LM,LF: tfc
            ServoOffset = new short[18];
            ServoInv = new short[18];
            ServoMap = new short[18];
        }

        public void Save(string file)
        {
            File.WriteAllText(file, JsonConvert.SerializeObject(this));
        }

        public static Settings Load(string file)
        {
            return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(file));
        }
    }

    public class FrameSettingsData
    {
        public Settings settings;
        public bool save;

        public byte[] ToArray()
        {
            var ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            foreach (var x in settings.ServoOffset) bw.Write(x);
            foreach (var y in settings.ServoInv) bw.Write(y);
            foreach (var z in settings.ServoMap) bw.Write(z);
            bw.Write(save);
            return ms.ToArray();
        }
    }
    class Program
    {
        static SerialProtocol sp = new SerialProtocol();
        static Stopwatch sw = new Stopwatch();


        static byte[] GetBinaryArray(uint[] servos)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            foreach (var servo in servos) bw.Write(servo);
            return ms.ToArray();
        }

        static void Main(string[] args)
        {
            sw.Start();
            sp.Create();
            sp.OnFrameReady += OnFrameReady;

            //var settings = Settings.Load("hexapod.settings.json");
            ////new Settings().Save("hexapod.settings.json");
            //var fs = new FrameSettingsData
            //{
            //    settings = settings,
            //    save = false
            //};
            //sp.SendFrame(FrameHeaderType.ESP32Debug, fs.ToArray());

            uint[] servos = new uint[26];
            servos[0] = 0xDEADBEAF;



            while (!Console.KeyAvailable)
            {
                sp.SendFrame(FrameHeaderType.STM32Debug, GetBinaryArray(servos));
                servos[1]++;
                sp.Loop();
            }
            sp.Destroy();
        }

        private static void OnFrameReady(object sender, FrameReadyEventArgs e)
        {
            Console.Write($"({sw.ElapsedMilliseconds}) FPS: {sp.FPS}. CPS: {e.Model.cps} Power: {e.Model.turnedOn}. [{e.Servos.Length}]: ");
            for (int i = 0; i < e.Servos.Length; i++)
                Console.Write($"{e.Servos[i]:X} ");
            Console.WriteLine();
        }
    }
}
