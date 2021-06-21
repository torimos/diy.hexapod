using SerialPortLib2;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public partial class MyScript : MonoBehaviour
{
    private SerialPortInput _port = new SerialPortInput(false);
    private FrameReader _fr = new FrameReader();
    private Hexapod hexapod = new Hexapod();
    Stopwatch last_leg_updated;
    Queue<uint[]> dataBuffer = new Queue<uint[]>();

    // Start is called before the first frame update
    void Start()
    {
        last_leg_updated = new Stopwatch();
        _port.SetPort("COM7", 115200);
        _port.ConnectionStatusChanged += SerialPort_ConnectionStatusChanged;
        _port.MessageReceived += SerialPort_MessageReceived;
        _fr.OnFrameReady += OnFrameReady;

        hexapod.Create();

        _port.Connect();
        last_leg_updated.Restart();
    }

    void SetServo(uint[] data, int segment, uint pos, uint delay = 100)
    {
        for (int i = 0; i < 6; i++)
            data[HexConfig.ServoMap[i, segment]] = pos | (delay << 16);
    }

    void SetAllServo(uint[] data, uint pos1, uint pos2, uint pos3, uint delay = 100)
    {
        Debug.Log($"SetAllServo c:{pos1} f:{pos2} t:{pos3}");
        SetServo(data, 0, pos1, delay);
        SetServo(data, 1, pos2, delay);
        SetServo(data, 2, pos3, delay);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var mockData = new uint[26];
            SetAllServo(mockData, 1500 - 450, 1500 - 450, 1500 - 450);
            dataBuffer.Enqueue(mockData);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            var mockData = new uint[26];
            SetAllServo(mockData, 1500, 1500, 1500);
            dataBuffer.Enqueue(mockData);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            var mockData = new uint[26];
            SetAllServo(mockData, 1500 + 450, 1500 + 450, 1500 + 450);
            dataBuffer.Enqueue(mockData);
        }

        if (dataBuffer.Count > 0) { 
            if (last_leg_updated.ElapsedMilliseconds > 10) {
                var _data = dataBuffer.Dequeue();
                if (Input.GetKey("1"))
                {
                    hexapod.UpdateLeg(_data, 1);
                    hexapod.UpdateLeg(_data, 3);
                    hexapod.UpdateLeg(_data, 5);
                }
                else if (Input.GetKey("2"))
                {
                    hexapod.UpdateLeg(_data, 0);
                    hexapod.UpdateLeg(_data, 2);
                    hexapod.UpdateLeg(_data, 4);
                }
                else
                {
                    hexapod.UpdateLeg(_data, 0);
                    hexapod.UpdateLeg(_data, 1);
                    hexapod.UpdateLeg(_data, 2);

                    hexapod.UpdateLeg(_data, 3);
                    hexapod.UpdateLeg(_data, 4);
                    hexapod.UpdateLeg(_data, 5);
                }

                last_leg_updated.Restart();
            }
        }
    }

    private void OnFrameReady(object sender, FrameReader.FrameReadyEventArgs args)
    {
        var d = new uint[26];
        args.Data.CopyTo(d, 0);
        dataBuffer.Enqueue(d);
    }

    private void SerialPort_MessageReceived(object sender, MessageReceivedEventArgs args)
    {
        _fr.Update(args.Data);
    }

    private void SerialPort_ConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
    {
    }

    void OnDestroy()
    {
        _port.Disconnect();
    }
}
