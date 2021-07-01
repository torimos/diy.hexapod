using System.IO;
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
