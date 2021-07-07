using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ServosService
{
    private Hexapod hexapod;
    private IEnumerator corutine;
    private UdpClient receiver;
    private Stopwatch sw = new Stopwatch();
    private int ups = 0;

    public void Create(MonoBehaviour parent, Hexapod hexapod, int port)
    {
        this.hexapod = hexapod;
        corutine = ServosStateReceiverRutine(port, 0.001f);
        parent.StartCoroutine(corutine);
        sw.Restart();
    }

    public void Destroy()
    {
        if (receiver != null)
        {
            receiver.Close();
            receiver.Dispose();
            receiver = null;
        }
    }

    private IEnumerator ServosStateReceiverRutine(int port, float waitTime)
    {
        receiver = new UdpClient(port);
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            if (receiver != null && receiver.Available > 0)
            {
                IPEndPoint ip = null;
                hexapod.ProcessFrameData(receiver.Receive(ref ip));
                ups++;
            }
            if (sw.ElapsedMilliseconds > 1000)
            {
                Debug.Log($"Updates per second: {ups}");
                ups = 0;
                sw.Restart();
            }
        }
    }
}
