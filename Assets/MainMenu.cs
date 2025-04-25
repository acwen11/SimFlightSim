using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // public TextMeshProUGUI errorMsg;
    public OptionsMenu options;
    public HowtoPlayMenu tut;

    void Awake()
    {
        options.opt_set = false;        
    }

    public void Select1p()
    {
        options.is_singleplayer = true;
        gameObject.SetActive(false);
        options.gameObject.SetActive(true);
    }

    public void Select2p()
    {
        options.is_singleplayer = false;
        gameObject.SetActive(false);
        options.gameObject.SetActive(true);
    }

    /*
    public void GoToOptions()
    {
        errorMsg.text = string.Empty;
        this.gameObject.SetActive(false);
        options.gameObject.SetActive(true);
    }
    */
    public void GoToOptions()
    {
        gameObject.SetActive(false);
        tut.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
