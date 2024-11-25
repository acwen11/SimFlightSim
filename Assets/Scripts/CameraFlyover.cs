using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFlyover : MonoBehaviour
{
    [Header("Flyover Path")]
    public float height;
    public float radius;
    public float ang_speed;
    public float angle;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(-radius, height, 0);
        transform.Rotate(angle, 90, 0, Space.World);
    }

    // Update is called once per frame
    void Update()
    {
        float theta = (ang_speed * Time.time) % 360.0f;
        //transform.position = new Vector3(-radius * Mathf.Cos(theta), height, -radius * Mathf.Sin(theta));
        Vector3 fly_pos = new Vector3(-radius * Mathf.Cos(theta * Mathf.PI / 180), height, -radius * Mathf.Sin(theta * Mathf.PI / 180));
        Quaternion fly_rot = Quaternion.Euler(angle, 90 - theta, 0);
        transform.SetLocalPositionAndRotation(fly_pos, fly_rot);
        //transform.rotation *= Quaternion.AngleAxis(-ang_speed * Time.deltaTime, Vector3.up);
        //transform.RotateAround(transform.position, Vector3.up, -ang_speed * radius * Time.deltaTime);
    }
}
