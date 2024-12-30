using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveCamera : MonoBehaviour
{
    public float cam_radius;
    public float cam_height;
    public float cam_angle;

    public float move_speed;
    public float rot_speed;

    [HideInInspector]
    public string save_name;

    // Input Vars
    InputAction moveAction;
    InputAction ssAction;

    private void Awake()
    {
        // Set initial position
        transform.position = new Vector3(0, cam_height, -cam_radius);
        transform.Rotate(cam_angle, 0, 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Viewer/Move");
        ssAction = InputSystem.actions.FindAction("Viewer/Screenshot");

        save_name = PlayerPrefs.GetString("simname");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        transform.RotateAround(Vector3.zero, Vector3.up, moveValue.x * move_speed);

        if (ssAction.WasReleasedThisFrame())
        {
            ScreenCapture.CaptureScreenshot(save_name + ".png");
        }
    }
}
