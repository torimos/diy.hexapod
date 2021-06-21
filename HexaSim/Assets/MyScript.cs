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
    }

    private void OnFrameReady(object sender, FrameReader.FrameReadyEventArgs args)
    {
        hexapod.ProcessFrameData(args.Data);
    }

    void OnDestroy()
    {
        frameReader.Destroy();
    }
}
