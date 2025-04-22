using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    public GameObject ufo1;
    public GameObject ufo2;
    private move_UFO ufo_controller1;
    private move_UFO ufo_controller2;

    private bool gameover = false;

    public Slider hp1;
    public Slider time1;
    public Slider hp2;
    public Slider time2;

    public GameObject winner1;
    public GameObject winner2;
    public GameObject loser1;
    public GameObject loser2;

    public Button menuButton;
    public GameObject inputWrapper;

    // Start is called before the first frame update
    void Start()
    {
        ufo_controller1 = ufo1.GetComponent<move_UFO>();
        ufo_controller2 = ufo2.GetComponent<move_UFO>();
    }

    // Update is called once per frame
    void Update()
    {
        hp1.value = ufo_controller1.health;
        hp2.value = ufo_controller2.health;
        time1.value = ufo_controller1.time;
        time2.value = ufo_controller2.time;

        if ((hp1.value <= 0 || hp2.value <= 0 || time1.value <= 0 || time2.value <= 0) && !gameover)
        {
            if (hp1.value <= 0 || time1.value <= 0)
            {
                Destroy(ufo1);
                winner2.SetActive(true);
                loser1.SetActive(true);
                menuButton.gameObject.SetActive(true);
                inputWrapper.SetActive(true);
                gameover = true;
            }
            if (hp2.value <= 0 || time2.value <= 0)
            {
                Destroy(ufo2);
                winner1.SetActive(true);
                loser2.SetActive(true);
                menuButton.gameObject.SetActive(true);
                inputWrapper.SetActive(true);
                gameover = true;
            }
        }
    }

    public void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
