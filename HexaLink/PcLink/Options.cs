using CommandLine;

namespace PcLink
{
    public class Options
    {
        [Option('t', "test", Required = false, HelpText = "Test mode")]
        public bool Test { get; set; }

        [Option('A', "address", Required = false, Default = "127.0.0.1", HelpText = "IP Address of Server")]
        public string Ip { get; set; }

        [Option('P', "port", Required = false, Default = 5555, HelpText = "Port of Server")]
        public int Port { get; set; }

        [Option('r', "record", Required = false, HelpText = "Recording file name")]
        public string RecordingName { get; set; }

        [Option('p', "play", Required = false, HelpText = "Play file name")]
        public string PlayName { get; set; }

        [Option('s', "speed", Required = false, Default = 1f, HelpText = "Playback speed")]
        public float PlaybackSpeed { get; set; }
    }
}
