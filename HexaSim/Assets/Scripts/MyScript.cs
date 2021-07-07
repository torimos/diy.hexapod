using CommandLine;
using UnityEngine;

public partial class MyScript : MonoBehaviour
{
    public class Options
    {
        [Option('p', "port", Required = false, Default = 5555, HelpText = "Port of Server")]
        public int Port { get; set; }
    }

    private Hexapod hexapod = new Hexapod();
    private ServosService servosService = new ServosService();

    void Start()
    {
        hexapod.Create(this);
        servosService.Create(this, hexapod, 5555);
    }

    void Run(int port)
    {
        
    }

    void FixedUpdate()
    {
        hexapod.Update();
    }

    void OnDestroy()
    {
        servosService.Destroy();
    }
}
