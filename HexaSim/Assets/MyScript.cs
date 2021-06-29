using UnityEngine;

public partial class MyScript : MonoBehaviour
{
    private SerialProtocol sp = new SerialProtocol();
    private Hexapod hexapod = new Hexapod();

    // Start is called before the first frame update
    void Start()
    {
        hexapod.Create(this);

        sp.Create();
        sp.OnFrameReady += OnFrameReady;
    }

    // Update is called once per frame
    void Update()
    {
        hexapod.Update();
        sp.Loop();
    }

    private void OnFrameReady(object sender, FrameReadyEventArgs args)
    {
        hexapod.ProcessFrameData(args);
    }

    void OnDestroy()
    {
        sp.Destroy();
    }
}
