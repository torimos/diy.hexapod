using UnityEngine;

public class Leg
{
    public bool mirror;

    public float coxaAngle;
    public float femurAngle;
    public float tibiaAngle;

    public GameObject legSegment;
    public GameObject coxaSegment;
    public GameObject coxaJoint;
    public GameObject coxa;
    public GameObject femurSegment;
    public GameObject femurJoint;
    public GameObject femur;
    public GameObject tibiaSegment;
    public GameObject tibiaJoint;
    public GameObject tibia;
    public GameObject tibiaEnd;

    public void Create(GameObject parent, string name, Vector3 pos, float angle, bool mirror = false)
    {
        this.mirror = mirror;
        legSegment = new GameObject(name);
        legSegment.transform.parent = parent.transform;
        legSegment.transform.localPosition = pos;
        legSegment.transform.localRotation = Quaternion.AngleAxis((mirror ? angle : 180 - angle) , new Vector3(0, 1, 0));

        // COXA

        coxaSegment = new GameObject("coxaSegment");
        coxaSegment.transform.parent = legSegment.transform;
        coxaSegment.transform.localPosition = new Vector3(0, 0, 0);

        coxaJoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coxaJoint.name = "coxaJoint";
        coxaJoint.transform.parent = coxaSegment.transform;
        coxaJoint.transform.localScale = new Vector3(HexConfig.coxaJointSize, HexConfig.coxaJointSize, HexConfig.coxaJointSize);
        coxaJoint.transform.localPosition = new Vector3(0, 0, 0);
        coxaJoint.GetComponent<Renderer>().material.color = new Color(128, 0, 0);

        coxa = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        coxa.name = "coxa";
        coxa.transform.parent = coxaSegment.transform;
        coxa.transform.localScale = new Vector3(HexConfig.coxaJointSize / 2, HexConfig.coxaLength / 2, HexConfig.coxaJointSize / 2);
        coxa.transform.localPosition = new Vector3(-HexConfig.coxaLength / 2, 0, 0);
        coxa.transform.localRotation = Quaternion.AngleAxis(90, new Vector3(0, 0, 1));
        coxa.GetComponent<Renderer>().material.color = Color.gray;

        // FEMUR

        femurSegment = new GameObject("femurSegment");
        femurSegment.transform.parent = coxaSegment.transform;
        femurSegment.transform.localPosition = new Vector3(-HexConfig.coxaLength, 0, 0);

        femurJoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        femurJoint.name = "femurJoint";
        femurJoint.GetComponent<Renderer>().material.color = new Color(0, 128, 0);
        femurJoint.transform.parent = femurSegment.transform;
        femurJoint.transform.localScale = new Vector3(HexConfig.otherJointSize, HexConfig.otherJointSize, HexConfig.otherJointSize);
        femurJoint.transform.localPosition = new Vector3(0, 0, 0);

        femur = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        femur.name = "femur";
        femur.transform.parent = femurSegment.transform;
        femur.transform.localScale = new Vector3(HexConfig.coxaJointSize / 3, HexConfig.femurLength / 2, HexConfig.coxaJointSize / 3);
        femur.transform.localPosition = new Vector3(0, HexConfig.femurLength / 2, 0);
        femur.GetComponent<Renderer>().material.color = Color.gray;

        // TIBIA

        tibiaSegment = new GameObject("tibiaSegment");
        tibiaSegment.transform.parent = femurSegment.transform;
        tibiaSegment.transform.localPosition = new Vector3(0, HexConfig.femurLength, 0);

        tibiaJoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tibiaJoint.name = "tibiaJoint";
        tibiaJoint.transform.parent = tibiaSegment.transform;
        tibiaJoint.transform.localScale = new Vector3(HexConfig.otherJointSize, HexConfig.otherJointSize, HexConfig.otherJointSize);
        tibiaJoint.transform.localPosition = new Vector3(0, 0, 0);
        tibiaJoint.GetComponent<Renderer>().material.color = new Color(0, 0, 128);

        tibia = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tibia.name = "tibia";
        tibia.transform.parent = tibiaSegment.transform;
        tibia.transform.localScale = new Vector3(HexConfig.coxaJointSize / 3, HexConfig.tibiaLength / 2, HexConfig.coxaJointSize / 3);
        tibia.transform.localPosition = new Vector3(0, HexConfig.tibiaLength / 2, 0);
        tibia.GetComponent<Renderer>().material.color = Color.gray;

        tibiaEnd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tibiaEnd.name = "tibiaEnd";
        tibiaEnd.transform.parent = tibiaSegment.transform;
        tibiaEnd.transform.localScale = new Vector3(HexConfig.otherJointSize, HexConfig.otherJointSize, HexConfig.otherJointSize);
        tibiaEnd.transform.localPosition = new Vector3(0, HexConfig.tibiaLength, 0);
        tibiaEnd.GetComponent<Renderer>().material.color = Color.yellow;
    }

    // Adobt servo angles to 3d model
    public void Update(float cAngle, float fAngle, float tAngle)
    {
        coxaAngle = cAngle;
        femurAngle = fAngle;
        tibiaAngle = tAngle;

        if (mirror)
        {
            femurAngle *= -1f;
            tibiaAngle *= -1f;
        }

        coxaSegment.transform.localRotation = Quaternion.AngleAxis(HexConfig.coxaOffsetAngle - coxaAngle, new Vector3(0, 1, 0));
        femurSegment.transform.localRotation = Quaternion.AngleAxis(HexConfig.femurOffsetAngle + femurAngle, new Vector3(0, 0, 1));
        tibiaSegment.transform.localRotation = Quaternion.AngleAxis(HexConfig.tibiaOffsetAngle - tibiaAngle, new Vector3(0, 0, 1));

        //Debug.Log($"{legSegment.name} {tibiaEnd.transform.position}");
    }
}
