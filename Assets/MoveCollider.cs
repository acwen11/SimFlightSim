using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCollider : MonoBehaviour
{
    public Transform head;
    public JumpscareFactory jumpscares;
    private IEnumerator jumpscare_cor;

    private CapsuleCollider body;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<CapsuleCollider>();
        jumpscare_cor = jumpscares.GetComponent<JumpscareFactory>().ManuelaJumpscare();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (jumpscares.gameObject.activeSelf && other.GetComponent<HorizonFlag>() != null)
        {
            Debug.Log("Sucked into Black Hole!");
            StartCoroutine(jumpscare_cor);
            Debug.Log("Coroutine done");
            jumpscare_cor = jumpscares.GetComponent<JumpscareFactory>().ManuelaJumpscare();
        }
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = head.position - Vector3.up * body.height / 2f;
    }
}
