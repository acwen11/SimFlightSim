using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class move_UFO : MonoBehaviour
{
    public bool isPlayer1;

    public float maxSpeed = 1;
    // public float maxPitchSpeed = 3;
    public float maxRotSpeed = 50;
    public float maxRollSpeed = 50;
    public float acceleration = 2;
    public float drag_const = 0.1f;
    public float Ggrav;
    public float clight;

    public float shoot_delay = 0.5f;
    private float shoot_timer = 0.5f;

    public float maxhealth = 100f;
    public float health; 
    public float maxtime = 60f;
    public float time; 

    public ParReader sim_pars;

    public Camera pCam;
    public CinemachineVirtualCamera fp_cam;
    public CinemachineVirtualCamera tp_cam;

    // Vector3 velocity;
    // float yawVelocity;
    // float pitchVelocity;
    // float currentSpeed;
    Vector3 currentVel;

    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform laserTransform;
    [SerializeField] private Transform spawnBulletPosition;

    // Input Vars
    InputAction moveAction;
    InputAction lookAction;
    InputAction upThrustAction;
    InputAction downThrustAction;
    InputAction rollAction;
    InputAction toggleCamAction;
    InputAction fireAction;

    void Awake()
    {
        // currentSpeed = maxSpeed;
        currentVel = Vector3.zero;
        health = maxhealth;
        time = maxtime;
    }

    private void Start()
    {
        moveAction = InputSystem.actions.FindAction("Player/Move");
        lookAction = InputSystem.actions.FindAction("Player/Look");
        upThrustAction = InputSystem.actions.FindAction("Player/Up Thrust");
        downThrustAction = InputSystem.actions.FindAction("Player/Down Thrust");
        rollAction = InputSystem.actions.FindAction("Player/Roll Mode");
        toggleCamAction = InputSystem.actions.FindAction("Player/Toggle Camera");
        fireAction = InputSystem.actions.FindAction("Player/Fire");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<laser_projectile>() != null)
        {
            health -= 5;
            Debug.Log("Hit! Health = " + health);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<HardSurfaceFlag>() != null)
        {
            health -= 0.05f;
            Debug.Log("Hit surface! Health = " + health);
        }

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

        // 3. Calculate Gravity. Newtonian for now
        int nsrcs = sim_pars.par_nSrcs;
        for (int ii=0; ii<nsrcs; ii++)
        {
            Vector3 rvec = transform.position - sim_pars.grav_masses[ii].gcoords;
            float rr = Mathf.Min(rvec.sqrMagnitude, 0.25f); // buffer radius a little bit
            Vector3 rhat = rvec.normalized;
            Vector3 Fg = transform.InverseTransformVector(-Ggrav * (sim_pars.grav_masses[ii].gmass / rr) * rhat);
            accel_vec += Fg;
        }

        // 4. Limit speed
        currentVel += accel_vec * Time.deltaTime;
        float fac = maxSpeed / currentVel.magnitude;
        if (fac < 1)
        {
            currentVel *= fac;
        }

        // 5. Update position
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

        // Shooting
        // Get look direction
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 camCenter = Vector2.zero;
        if (isPlayer1) {
            camCenter = new Vector2(Screen.width / 4f, Screen.height / 2f);
        }
        else
        {
            camCenter = new Vector2(3f * Screen.width / 4f, Screen.height / 2f);
        }
        Ray ray = pCam.ScreenPointToRay(camCenter);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

        if (fireAction.WasPressedThisFrame() && shoot_timer >= shoot_delay)
        {
            Debug.Log("Fire Pressed");
            shoot_timer = 0f;
            Vector3 aimdir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            Instantiate(laserTransform, spawnBulletPosition.position, Quaternion.LookRotation(aimdir, Vector3.up));
        }
        shoot_timer = Mathf.Min(shoot_delay, shoot_timer + Time.deltaTime);

        // Time dilation
        float invalpsq = 1f; // use superimposed nonspinning Kerr-Schild lapse
        for (int ii = 0; ii < nsrcs; ii++)
        {
            Vector3 rvec = transform.position - sim_pars.grav_masses[ii].gcoords;
            float rad = Mathf.Min(rvec.magnitude, 0.25f); // buffer radius a little bit
            invalpsq += (2 * Ggrav * sim_pars.grav_masses[ii].gmass) / (clight * clight * rad);
        }
        float alp = 1 / Mathf.Sqrt(invalpsq);
        if (isPlayer1)
        {
            Debug.Log("lapse = " + alp);
        }
        time -= alp * Time.deltaTime;
    }
}
