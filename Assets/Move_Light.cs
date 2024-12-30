using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Move_Light : MonoBehaviour
{
    public float dl_radius;
    public float dl_height;
    public float dl_angle;

    public float move_speed;

    // Input Vars
    InputAction moveAction;

    private void Awake()
    {
        // Set initial position
        transform.position = new Vector3(0, dl_height, -dl_radius);
        transform.Rotate(dl_angle, 0, 0);
    }
    // Start is called before the first frame update
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Viewer/Move Light");
    }

    // Update is called once per frame
    void Update()
    {
        float moveValue = moveAction.ReadValue<float>();
        transform.RotateAround(Vector3.zero, Vector3.up, moveValue * move_speed);
    }
}
