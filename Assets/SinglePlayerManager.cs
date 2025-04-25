using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SinglePlayerManager : MonoBehaviour
{
    public GameObject ufo1;
    private move_UFO ufo_controller1;

    private bool gameover = false;

    public Slider time1;

    public GameObject donetxt;

    public Button menuButton;
    public GameObject inputWrapper;

    // Start is called before the first frame update
    void Start()
    {
        ufo_controller1 = ufo1.GetComponent<move_UFO>();
    }

    // Update is called once per frame
    void Update()
    {
        time1.value = ufo_controller1.time;

        if (time1.value <= 0)
        {
            // Force update
            time1.value = ufo_controller1.time;

            //Destroy(ufo1);
            donetxt.SetActive(true);
            //menuButton.gameObject.SetActive(true);
            //inputWrapper.SetActive(true);
            gameover = true;
        }
        if (ufo_controller1.time <= -4f)
        {
            SceneManager.LoadScene("Menu");
        }

    }

    public void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
