using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class move_UFO_singleplayer : MonoBehaviour
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
    public float health = 100f; 
    public float maxtime = 60f;
    public float time = 60f; 

    private ParReader sim_pars;

    public Camera pCam;
    public CinemachineVirtualCamera fp_cam;
    public CinemachineVirtualCamera tp_cam;

    public Transform spawnpt;

    // Vector3 velocity;
    // float yawVelocity;
    // float pitchVelocity;
    // float currentSpeed;
    Vector3 currentVel;

    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    // [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform laserTransform;
    [SerializeField] private Transform spawnBulletPosition;

    private HUDManager myHUDManager;

    // Input Vars
    //InputAction moveAction;
    private Vector2 moveValue;
    //InputAction lookAction;
    private Vector2 rotValue;
    // InputAction upThrustAction;
    private bool upThrust = false;
    // InputAction downThrustAction;
    private bool downThrust = false;
    // InputAction rollAction;
    private bool rollMode = false;
    // InputAction toggleCamAction;
    private bool toggleCam;
    // InputAction fireAction;
    private bool fire;

    // Input Messages
    public void OnMove(InputAction.CallbackContext context)
    {
        moveValue = context.ReadValue<Vector2>(); 
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        rotValue = context.ReadValue<Vector2>(); 
    }

    public void OnUpThrust(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            upThrust = true;
        }
        else
        {
            upThrust = false;
        }
    }

    public void OnDownThrust(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            downThrust = true;
        }
        else
        {
            downThrust = false;
        }
    }

    //public void OnRollMode(InputAction.CallbackContext context)
    public void OnRollMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            rollMode = true;
        }
        else
        {
            rollMode = false;
        }
    }

    public void OnToggleCamera(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            toggleCam = true;
        }
        else
        {
            fire = false;
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            fire = true;
        }
        else
        {
            fire = false;
        }
    }

    void Awake()
    {
        sim_pars = FindFirstObjectByType<ParReader>();
        myHUDManager = FindFirstObjectByType<HUDManager>();
        if (isPlayer1)
        {
            myHUDManager.ufo1 = gameObject;
        }
        else
        {
            myHUDManager.ufo2 = gameObject;
        }

        // currentSpeed = maxSpeed;
        currentVel = Vector3.zero;
        health = maxhealth;
        time = maxtime;
    }

    private void Start()
    {
        //moveAction = InputSystem.actions.FindAction("Player/Move");
        // lookAction = InputSystem.actions.FindAction("Player/Look");
        // upThrustAction = InputSystem.actions.FindAction("Player/Up Thrust");
        // downThrustAction = InputSystem.actions.FindAction("Player/Down Thrust");
        // rollAction = InputSystem.actions.FindAction("Player/Roll Mode");
        // toggleCamAction = InputSystem.actions.FindAction("Player/Toggle Camera");
        // fireAction = InputSystem.actions.FindAction("Player/Fire");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<laser_projectile>() != null)
        {
            health -= 5f;
            Debug.Log("Hit! Health = " + health);
        }
        else if (other.GetComponent<HorizonFlag>() != null)
        {
            health = 0;
            Debug.Log("Sucked into black hole!");
            transform.position = spawnpt.position;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<HardSurfaceFlag>() != null)
        {
            health -= 0.1f;
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
        //Vector2 moveValue = moveAction.ReadValue<Vector2>();
        az = acceleration * moveValue.y;
        ax = acceleration * moveValue.x;
        if (upThrust)
        {
            ay += acceleration;
        }
        if (downThrust)
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
            float rr = Mathf.Max(rvec.sqrMagnitude, 0.005f); // buffer radius a little bit
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
        // Vector2 rotValue = lookAction.ReadValue<Vector2>();
        if (rollMode)
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
        if (toggleCam)
        {
            int fp_prio = fp_cam.m_Priority;
            int tp_prio = tp_cam.m_Priority;
            fp_cam.m_Priority = tp_prio;
            tp_cam.m_Priority = fp_prio;
            toggleCam = false;
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
            // debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }

        if (fire && shoot_timer >= shoot_delay)
        {
            shoot_timer = 0f;
            Vector3 aimdir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            Instantiate(laserTransform, spawnBulletPosition.position, Quaternion.LookRotation(aimdir, Vector3.up));
            fire = false;
        }
        shoot_timer = Mathf.Min(shoot_delay, shoot_timer + Time.deltaTime);

        // Time dilation
        float invalpsq = 1f; // use superimposed nonspinning Kerr-Schild lapse
        for (int ii = 0; ii < nsrcs; ii++)
        {
            Vector3 rvec = transform.position - sim_pars.grav_masses[ii].gcoords;
            float rad = Mathf.Max(rvec.magnitude, 0.005f); // buffer radius a little bit
            invalpsq += (2 * Ggrav * sim_pars.grav_masses[ii].gmass) / (clight * clight * rad);
        }
        float alp = 1f / Mathf.Sqrt(invalpsq);
        if (isPlayer1)
        {
            //Debug.Log("lapse = " + alp);
        }
        time -= alp * Time.deltaTime;
    }
}
