using UnityEngine;

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{
    [Range(0, 24)]
    public float distance = 1.0f;
    public Vector3 rotation;
    public Transform target;

    const float rstep = 80f;
    const float dstep = 2f;

    void Update()
    {
        if (target == null)
            return;

        if (Input.GetKey(KeyCode.A))
            rotation.y += rstep * Time.deltaTime;
        else if (Input.GetKey(KeyCode.D))
            rotation.y -= rstep * Time.deltaTime;
        if (Input.GetKey(KeyCode.W))
            rotation.x -= rstep * Time.deltaTime;
        else if (Input.GetKey(KeyCode.S))
            rotation.x += rstep * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q))
            distance += dstep * Time.deltaTime;
        else if (Input.GetKey(KeyCode.E))
            distance -= dstep * Time.deltaTime;

        transform.position = target.position + Quaternion.Euler(rotation) * (Vector3.forward * distance);
        transform.LookAt(target);
    }
}
