using System;
using System.IO;

public enum FrameHeaderType
{
    ESP32Debug = 0x412B,
    STM32Debug = 0xFA2C,
    Unknown = 0xFFFF
}

public class FrameReadyEventArgs
{
    public struct XYZ
    {
        public double x, y, z;
        public override string ToString()
        {
            return $"{x:F2}:{y:F2}:{z:F2}";
        }
    }
    public struct ModelData
    {
        public XYZ tlen;
        public XYZ pos;
        public XYZ rot;
        public bool turnedOn;
    }
    public ModelData Model;
    public uint[] Servos;
}
public class FrameReadyEventArgsBuilder
{
    public static FrameReadyEventArgs Create(byte[] data)
    {
        var args = new FrameReadyEventArgs() { Servos = new uint[26] };
        var br = new BinaryReader(new MemoryStream(data));
        Buffer.BlockCopy(data, 0, args.Servos, 0, args.Servos.Length * 4);
        br.BaseStream.Seek(args.Servos.Length * 4, SeekOrigin.Begin);
        args.Model.tlen.x = br.ReadDouble();
        args.Model.tlen.y = br.ReadDouble();
        args.Model.tlen.z = br.ReadDouble();
        args.Model.pos.x = br.ReadDouble();
        args.Model.pos.y = br.ReadDouble();
        args.Model.pos.z = br.ReadDouble();
        args.Model.rot.x = br.ReadDouble();
        args.Model.rot.y = br.ReadDouble();
        args.Model.rot.z = br.ReadDouble();
        args.Model.turnedOn = br.ReadBoolean();
        return args;
    }
}
