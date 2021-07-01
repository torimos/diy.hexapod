using System.IO;
using System.Text.Json;

public class Settings
{
    public short[] ServoOffset { get; set; }
    public short[] ServoInv { get; set; }
    public short[] ServoMap { get; set; }
    public Settings()
    {
        // RR,RM,RF,LR,LM,LF: tfc
        ServoOffset = new short[18];
        ServoInv = new short[18];
        ServoMap = new short[18];
    }

    public void Save(string file)
    {
        File.WriteAllText(file, JsonSerializer.Serialize(this));
    }

    public static Settings Load(string file)
    {
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(file));
    }
}
