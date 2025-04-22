using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpscareFactory : MonoBehaviour
{

    public GameObject jumpscareCanvas;
    public Transform player;
    public Transform spawn;

    private AudioSource audio;

    public IEnumerator ManuelaJumpscare()
    {
        audio.Play();
        jumpscareCanvas.SetActive(true);
        yield return new WaitForSeconds(2);
        jumpscareCanvas.SetActive(false);
        player.position = spawn.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
