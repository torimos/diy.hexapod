using System.IO;
using Newtonsoft.Json;

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
