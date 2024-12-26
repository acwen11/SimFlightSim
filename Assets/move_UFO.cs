using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class move_UFO : MonoBehaviour
{
    public float maxSpeed = 1;
    // public float maxPitchSpeed = 3;
    public float maxRotSpeed = 50;
    public float maxRollSpeed = 50;
    public float acceleration = 2;
    public float drag_const = 0.1f;

    // public float smoothSpeed = 3;
    // public float smoothTurnSpeed = 3;

    public CinemachineVirtualCamera fp_cam;
    public CinemachineVirtualCamera tp_cam;

    // Vector3 velocity;
    // float yawVelocity;
    // float pitchVelocity;
    // float currentSpeed;
    Vector3 currentVel;

    // Input Vars
    InputAction moveAction;
    InputAction lookAction;
    InputAction upThrustAction;
    InputAction downThrustAction;
    InputAction rollAction;
    InputAction toggleCamAction;

    void Awake()
    {
        // currentSpeed = maxSpeed;
        currentVel = Vector3.zero;
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        upThrustAction = InputSystem.actions.FindAction("Up Thrust");
        downThrustAction = InputSystem.actions.FindAction("Down Thrust");
        rollAction = InputSystem.actions.FindAction("Roll Mode");
        toggleCamAction = InputSystem.actions.FindAction("Toggle Camera");
    }

    void Update()
    {
        // Init motion components
        float ax    = 0f;
        float ay    = 0f;
        float az    = 0f;
        float omyaw   = 0f;
        float ompitch = 0f;
        float omroll  = 0f;

        // Calculate Linear Motion
        // 1. Read input
        // NOTE: difference between Input system axes and object local axes
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        az = acceleration * moveValue.y;
        ax = acceleration * moveValue.x;
        if (upThrustAction.IsPressed())
        {
            ay += acceleration;
        }
        if (downThrustAction.IsPressed())
        {
            ay -= acceleration;
        }
        Vector3 accel_vec = new Vector3(ax, ay, az);

        // 2. Apply damping
        accel_vec -= drag_const * currentVel;

        // 3. Limit speed
        currentVel += accel_vec * Time.deltaTime;
        float fac = maxSpeed / currentVel.magnitude;
        if (fac < 1)
        {
            currentVel *= fac;
        }

        // 4. Update position
        transform.Translate(currentVel * Time.deltaTime);

        // Calculate Rotation
        // 1. Get input
        Vector2 rotValue = lookAction.ReadValue<Vector2>();
        if (rollAction.IsPressed())
        {
            omroll = -maxRollSpeed * rotValue.x; // Personal preference for direction
        }
        else
        {
            omyaw = maxRotSpeed * rotValue.x;
        }
        ompitch = -maxRotSpeed * rotValue.y; // Personal preference for direction

        // 2. Update rotation
        Vector3 omEuler = new Vector3(ompitch, omyaw, omroll);
        transform.Rotate(omEuler * Time.deltaTime);

        // Additional actions
        if (toggleCamAction.WasReleasedThisFrame())
        {
            int fp_prio = fp_cam.m_Priority;
            int tp_prio = tp_cam.m_Priority;
            fp_cam.m_Priority = tp_prio;
            tp_cam.m_Priority = fp_prio;
        }

        /*
        float accelDir = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            accelDir -= 1;
        }
        if (Input.GetKey(KeyCode.E))
        {
            accelDir += 1;
        }
        if (Input.GetKeyUp(KeyCode.T))
        {
            int fp_prio = fp_cam.m_Priority;
            int tp_prio = tp_cam.m_Priority;
            fp_cam.m_Priority = tp_prio;
            tp_cam.m_Priority = fp_prio;
        }

        currentSpeed += acceleration * Time.deltaTime * accelDir;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        float speedPercent = currentSpeed / maxSpeed;

        Vector3 targetVelocity = transform.forward * currentSpeed;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * smoothSpeed);

        float targetPitchVelocity = Input.GetAxisRaw("Vertical") * maxPitchSpeed;
        pitchVelocity = Mathf.Lerp(pitchVelocity, targetPitchVelocity, Time.deltaTime * smoothTurnSpeed);

        float targetYawVelocity = Input.GetAxisRaw("Horizontal") * maxTurnSpeed;
        yawVelocity = Mathf.Lerp(yawVelocity, targetYawVelocity, Time.deltaTime * smoothTurnSpeed);
        transform.localEulerAngles += (Vector3.up * yawVelocity + Vector3.left * pitchVelocity) * Time.deltaTime * speedPercent;
        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
        */

    }
}
