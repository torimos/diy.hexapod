
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace PcLink
{
    class Demo2
    {
        public const int SERVOS_FRAME_SIZE = 26 * 4;

        public static void Run(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    if (o.Test)
                    {
                        RunTestServer(o).GetAwaiter().GetResult();
                    }
                    else
                    {
                        RunClient(o).GetAwaiter().GetResult();
                    }
                });
        }

        private static async Task RunTestServer(Options o)
        {
            Console.WriteLine("Press enter to stop server");
            UdpClient receiver = new UdpClient(o.Port);
            IPEndPoint ip = null;
            while(!Console.KeyAvailable)
            {
                if (receiver.Available > 0)
                {
                    byte[] data = receiver.Receive(ref ip);
                    uint[] servosData = new uint[data.Length >> 2];
                    Buffer.BlockCopy(data, 0, servosData, 0, data.Length);
                    Console.Write($"From {ip} [{servosData.Length}]:");
                    for (int i = 0; i < servosData.Length; i++)
                        Console.Write($"{servosData[i]:X8} ");
                    Console.WriteLine();
                }
            }
            receiver.Close();

            Console.WriteLine("Press enter to exit HexaSim client");
            Console.ReadLine();
        }

        private static async Task RunClient(Options o)
        {

            UdpClient client = new UdpClient(o.Ip, o.Port);
            FileStream playStream = !string.IsNullOrEmpty(o.PlayName) ? new FileStream(o.PlayName, FileMode.Open) : null;
            FileStream recStream = !string.IsNullOrEmpty(o.RecordingName) ? new FileStream(o.RecordingName, FileMode.Append) : null;

            SerialProtocol sp = null;

            if (playStream != null)
            {
                if (playStream.Length >= SERVOS_FRAME_SIZE)
                {
                    int totalFrameCount = (int)(playStream.Length / SERVOS_FRAME_SIZE);
                    Console.WriteLine($"Starting playback of {o.PlayName}, Total frames: {totalFrameCount}");
                    int f = 0;
                    var br = new BinaryReader(playStream);
                    while(true)
                    {
                        var data = br.ReadBytes(26*4);
                        if (data.Length == SERVOS_FRAME_SIZE)
                        {
                            client.Send(data, data.Length);
                            int frame_delay = 0;
                            var angles = new List<float>();
                            for (int i=0;i<data.Length;i+=4)
                            {
                                int a = data[i] | (data[i + 1] << 8);
                                int d = data[i+2] | (data[i+3] << 8);
                                if (d > frame_delay) frame_delay = d;
                                
                                float deg = (a - 1500) / 10f;
                                angles.Add(deg);
                            }
                            Console.WriteLine($"Sent frame {++f} of {totalFrameCount}, delay={frame_delay}");
                            Console.Write($"Servos:");
                            for (int i = 0; i < angles.Count; i++) Console.Write($"{i}={angles[i]:F2} ");
                            Console.WriteLine();
                            Thread.Sleep((int)(frame_delay / o.PlaybackSpeed));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                sp = new SerialProtocol();
                sp.Start();
                sp.OnFrameReady += (object sender, FrameReadyEventArgs e) =>
                {
                    byte[] byteArray = new byte[e.Servos.Length * 4];
                    Buffer.BlockCopy(e.Servos, 0, byteArray, 0, byteArray.Length);
                    client.Send(byteArray, byteArray.Length);
                    if (recStream != null)
                    {
                        recStream.Write(byteArray, 0, byteArray.Length);
                    }
                };
            }

            Console.WriteLine("Press enter to exit HexaSim client");
            Console.ReadLine();
            client.Close();
            if (sp != null)
            {
                sp.Stop();
            }
            if (recStream != null)
            {
                recStream.Flush();
                recStream.Close();
            }
            if (playStream != null)
            {
                playStream.Flush();
                playStream.Close();
            }

            Console.WriteLine("Exiting ....");
        }
    }
}
