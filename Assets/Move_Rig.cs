using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class Move_Rig: MonoBehaviour
{
    public float speed = 1;
    public float maxRollSpeed = 50;
    public float maxRotSpeed = 50;

    public ParReader sim_pars;

    // Input Vars
    InputAction moveAction;
    // InputAction lookAction;
    InputAction upThrustAction;
    InputAction downThrustAction;
    InputAction rollTriggerAction;
    // InputAction toggleCamAction;

    void Awake()
    {
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("XRI LeftHand Interaction/Move");
        // lookAction = InputSystem.actions.FindAction("XRI Head/Rotation");
        upThrustAction = InputSystem.actions.FindAction("XRI RightHand Interaction/Select");
        downThrustAction = InputSystem.actions.FindAction("XRI LeftHand Interaction/Select");
        rollTriggerAction = InputSystem.actions.FindAction("XRI LeftHand Interaction/Activate");
    }

    void Update()
    {
        // Init motion components
        float vx = 0f;
        float vy = 0f;
        float vz = 0f;
        float omyaw = 0f;
        float ompitch = 0f;
        float omroll = 0f;

        // Calculate Linear Motion
        // 1. Read input
        // NOTE: difference between Input system axes and object local axes
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        if (rollTriggerAction.IsPressed())
        {
            omroll = -maxRollSpeed * moveValue.x; // Personal preference for direction
        }
        /*
        else
        {
            vz = speed * moveValue.y;
            vx = speed * moveValue.x;
        }
        */

        if (upThrustAction.IsPressed())
        {
            vy += speed;
        }
        if (downThrustAction.IsPressed())
        {
            vy -= speed;
        }
        Vector3 vel_vec = new Vector3(vx, vy, vz);

        transform.Translate(vel_vec * Time.deltaTime);

        // Calculate Rotation
        // 1. Get input
        // Vector3 rotLook = lookAction.ReadValue<Quaternion>().eulerAngles;
        // transform.Rotate(rotLook, Space.World);
        // Quaternion rotLook = lookAction.ReadValue<Quaternion>();
        // transform.rotation = rotLook;

        // 2. Update rotation
        Vector3 omEuler = new Vector3(0, 0, omroll);
        transform.Rotate(omEuler * Time.deltaTime);

    }
}

