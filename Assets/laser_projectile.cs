using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser_projectile : MonoBehaviour
{
    private Rigidbody bulletBody;
    public float clight;

    private void Awake()
    {
        bulletBody = GetComponent<Rigidbody>();
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
        
    }
}
