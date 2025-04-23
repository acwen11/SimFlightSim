using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser_projectile : MonoBehaviour
{
    private Rigidbody bulletBody;
    public float clight;
    public float Ggrav;
    private ParReader pars;

    private void Awake()
    {
        bulletBody = GetComponent<Rigidbody>();
        pars = FindFirstObjectByType<ParReader>();
    }

    // Start is called before the first frame update
    void Start()
    {
        bulletBody.velocity = transform.forward * clight;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // "Gravitational Lensing"
        int nsrcs = pars.par_nSrcs;
        for (int ii = 0; ii < nsrcs; ii++)
        {
            Vector3 rvec = transform.position - pars.grav_masses[ii].gcoords;
            float rr = Mathf.Min(rvec.sqrMagnitude, 0.25f); // buffer radius a little bit
            Vector3 rhat = rvec.normalized;
            // Newtonian lensing eq with factor 2 correction
            Vector3 dv_perp = 2 * Time.deltaTime * (-Ggrav * (pars.grav_masses[ii].gmass / rr) * rhat);
            Debug.Log("Current vel = " + bulletBody.velocity + "; dv = " + dv_perp);
            bulletBody.velocity += dv_perp;
        }

        // Align shape with motion
        transform.rotation = Quaternion.LookRotation(bulletBody.velocity);
        transform.Rotate(90, 0, 0);
    }
}
