using UnityEngine;

public partial class MyScript : MonoBehaviour
{
    private FrameReader frameReader = new FrameReader();
    private Hexapod hexapod = new Hexapod();

    // Start is called before the first frame update
    void Start()
    {
        hexapod.Create(this);

        frameReader.Create();
        frameReader.OnFrameReady += OnFrameReady;
    }

    // Update is called once per frame
    void Update()
    {
        hexapod.Update();
        frameReader.Loop();
    }

    private void OnFrameReady(object sender, FrameReadyEventArgs args)
    {
        hexapod.ProcessFrameData(args);
    }

    void OnDestroy()
    {
        frameReader.Destroy();
    }
}
