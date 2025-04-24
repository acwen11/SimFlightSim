using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJoinScript : MonoBehaviour
{
    public Transform SpawnPoint1, SpawnPoint2;
    public GameObject Player1, Player2;
    // public HUDManager myHUDManager;

    private void Awake()
    {
        Instantiate(Player1, SpawnPoint1.position, SpawnPoint1.rotation);
        Instantiate(Player2, SpawnPoint2.position, SpawnPoint2.rotation);
        // myHUDManager.ufo1 = Player1;
        // myHUDManager.ufo2 = Player2;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
