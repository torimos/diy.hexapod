using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Hexapod
{
    private Leg[] legs = new Leg[HexConfig.LegsCount];
    private ServoState[] servos = new ServoState[26];
    private GameObject hexapod;
    private Stopwatch last_leg_updated;
    private IEnumerator servoUpdateCorutine;

    Vector3? lastHexaPos = null;
    Vector3? hexaNewPosition = null;
    float? lastHexaRotY = null;
    float? hexaNewRotY = null;

    public void Create(MonoBehaviour parent)
    {
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
        UpdateBody();
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

        UpdateBody();
    }

    private void UpdateBody()
    {
        //float minLegY = 0;
        //for (int i = 0; i < legs.Length; i++)
        //{
        //    minLegY += legs[i].tibiaEnd.transform.position.y;
        //}
        //minLegY /= 6;
        //hexapod.transform.position = new Vector3(0, -minLegY, 0);

        //if (hexaNewPosition != null)
        //{
        //    hexapod.transform.position = hexaNewPosition.Value;
        //    lastHexaPos = hexapod.transform.position;
        //    hexaNewPosition = null;
        //}

        //if (hexaNewRotY != null)
        //{
        //    hexapod.transform.rotation = Quaternion.AngleAxis(hexaNewRotY.Value, new Vector3(0, 1, 0));
        //    lastHexaRotY = hexaNewRotY.Value;
        //    hexaNewRotY = null;
        //}
    }

    public void ProcessFrameData(FrameReadyEventArgs args)
    {
        for (int i = 0; i < servos.Length; i++)
        {
            servos[i].ProcessData(args.Servos[i]);
        }
        var model = args.Model;

        float newPosX = 0, newPosY, newPosZ = 0, newRotY = 0;
        if (lastHexaPos != null)
        {
            newPosX = lastHexaPos.Value.x;
            newPosZ = lastHexaPos.Value.z;
        }
        newPosY = (float)(model.pos.y / 100.0f) + HexConfig.otherJointSize / 2;
        newPosX += (float)(model.tlen.x / 500.0f);
        newPosZ += (float)(model.tlen.z / 500.0f);
        if (lastHexaRotY != null)
        {
            newRotY = lastHexaRotY.Value;
        }
        newRotY -= (float)(model.tlen.y / 10.0f);

        hexaNewRotY = newRotY;
        hexaNewPosition = new Vector3(newPosX, newPosY, newPosZ);
        Debug.Log($"Model tlen={model.tlen} rot={model.rot} pos={model.pos} pwr={model.turnedOn}");
    }

    private void Create3DModel()
    {
        hexapod = new GameObject("hexapod");
        hexapod.transform.position = new Vector3(0, 0, 0);

        var hexaBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hexaBase.name = "hexaBase";
        hexaBase.transform.parent = hexapod.transform;
        hexaBase.transform.localScale = new Vector3(HexConfig.legsZOffset * 2, HexConfig.bodyHeight / 2, HexConfig.legsZOffset * 2);
        hexaBase.transform.localPosition = new Vector3(0, HexConfig.bodyOffset, 0);
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
