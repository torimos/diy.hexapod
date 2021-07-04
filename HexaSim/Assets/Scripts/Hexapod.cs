using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Hexapod
{
    private Leg[] legs = new Leg[HexConfig.LegsCount];
    private ServoState[] servos = new ServoState[26];
    public MonoBehaviour parent;
    private Stopwatch last_leg_updated;
    private IEnumerator servoUpdateCorutine;

    public void Create(MonoBehaviour parent)
    {
        this.parent = parent;
        last_leg_updated = new Stopwatch();
        last_leg_updated.Restart();
        for (int i = 0; i < servos.Length; i++)
            servos[i] = new ServoState();

        Create3DModel();
        Reset();

        servoUpdateCorutine = ServoUpdateRutine(0.02f);
        parent.StartCoroutine(servoUpdateCorutine);
    }

    private IEnumerator ServoUpdateRutine(float waitTime)
    {
        while(true)
        {
            yield return new WaitForSeconds(waitTime);
            for (int i = 0; i < servos.Length; i++)
            {
                servos[i].Update();
            }
        }
    }

    public void Update()
    {
        for (int i = 0; i < HexConfig.LegsCount; i++)
        {
            uint sdC = (uint)servos[HexConfig.ServoMap[i, 2]].position;
            uint sdF = (uint)servos[HexConfig.ServoMap[i, 1]].position;
            uint sdT = (uint)servos[HexConfig.ServoMap[i, 0]].position;

            if (sdC == 0 || sdF == 0 || sdT == 0)
            {
                legs[i].Reset();
                continue;
            }

            float d = 10f;
            float c = ((int)(sdC & 0xFFFF) - 1500) / d;
            float f = ((int)(sdF & 0xFFFF) - 1500) / d;
            float t = ((int)(sdT & 0xFFFF) - 1500) / d;

            legs[i].Update(c, f, t);
        }
    }

    public void Reset()
    {
        legs[0].Update(HexConfig.coxaInitAngle, HexConfig.femurInitAngle, HexConfig.tibiaInitAngle);
        legs[1].Update(HexConfig.coxaInitAngle, HexConfig.femurInitAngle, HexConfig.tibiaInitAngle);
        legs[2].Update(HexConfig.coxaInitAngle, HexConfig.femurInitAngle, HexConfig.tibiaInitAngle);

        legs[3].Update(-HexConfig.coxaInitAngle, -HexConfig.femurInitAngle, -HexConfig.tibiaInitAngle);
        legs[4].Update(-HexConfig.coxaInitAngle, -HexConfig.femurInitAngle, -HexConfig.tibiaInitAngle);
        legs[5].Update(-HexConfig.coxaInitAngle, -HexConfig.femurInitAngle, -HexConfig.tibiaInitAngle);
    }

    public void UpdateLeg(uint[] servoData, int legIdx)
    {
        uint sdC = servoData[HexConfig.ServoMap[legIdx, 2]];
        uint sdF = servoData[HexConfig.ServoMap[legIdx, 1]];
        uint sdT = servoData[HexConfig.ServoMap[legIdx, 0]];

        if (sdC == 0 || sdF == 0 || sdT == 0)
        {
            legs[legIdx].Reset();
            return;
        }

        float d = 10f;
        float c = ((int)(sdC & 0xFFFF) - 1500) / d;
        float f = ((int)(sdF & 0xFFFF) - 1500) / d;
        float t = ((int)(sdT & 0xFFFF) - 1500) / d;

        legs[legIdx].Update(c, f, t);
    }

    public void ProcessFrameData(FrameReadyEventArgs args)
    {
        for (int i = 0; i < servos.Length; i++)
        {
            servos[i].ProcessData(args.Servos[i]);
        }
        var model = args.Model;
        Debug.Log($"Model tlen={model.tlen} rot={model.rot} pos={model.pos} pwr={model.turnedOn}");
    }

    private void Create3DModel()
    {
        var hexapod = parent.gameObject;
        var hrb = hexapod.AddComponent<Rigidbody>();
        hrb.mass = 2;

        var hexaBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hexaBase.name = "hexaBase";
        hexaBase.transform.parent = hexapod.transform;
        hexaBase.transform.localScale = new Vector3(HexConfig.legsZOffset * 2, HexConfig.bodyHeight / 2, HexConfig.legsZOffset * 2);
        hexaBase.transform.localPosition = new Vector3(0, HexConfig.bodyOffset, 0); ;
        var hbcc = hexaBase.GetComponent<CapsuleCollider>();
        hbcc.enabled = false;
        hexaBase.GetComponent<Renderer>().material.color = Color.gray;

        var hexaHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hexaHead.name = "hexaHead";
        hexaHead.transform.parent = hexaBase.transform;
        hexaHead.transform.localScale = new Vector3(0.25f, 0.7f, 0.25f);
        hexaHead.transform.localPosition = new Vector3(0, 0, -HexConfig.legsXOffset + 0.1f);
        hexaHead.GetComponent<Renderer>().material.color = Color.black;

        for (int i = 0; i < legs.Length; i++) legs[i] = new Leg();
        legs[0].Create(hexapod, "RR", new Vector3(-HexConfig.legsXOffset, HexConfig.legsOffsetY, HexConfig.legsZOffset), HexConfig.legsAngleOffset, true);
        legs[1].Create(hexapod, "RM", new Vector3(-HexConfig.legsXOffset * 2, HexConfig.legsOffsetY, 0), 0, true);
        legs[2].Create(hexapod, "RF", new Vector3(-HexConfig.legsXOffset, HexConfig.legsOffsetY, -HexConfig.legsZOffset), -HexConfig.legsAngleOffset, true);

        legs[3].Create(hexapod, "LR", new Vector3(HexConfig.legsXOffset, HexConfig.legsOffsetY, HexConfig.legsZOffset), HexConfig.legsAngleOffset);
        legs[4].Create(hexapod, "LM", new Vector3(HexConfig.legsXOffset * 2, HexConfig.legsOffsetY, 0), 0);
        legs[5].Create(hexapod, "LF", new Vector3(HexConfig.legsXOffset, HexConfig.legsOffsetY, -HexConfig.legsZOffset), -HexConfig.legsAngleOffset);
    }
}
