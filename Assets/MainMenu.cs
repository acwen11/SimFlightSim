using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI errorMsg;
    public OptionsMenu options;

    void Awake()
    {
        options.opt_set = false;        
    }

    public void StartGame()
    {
        if (!options.opt_set)
        {
            errorMsg.text = "Error: options not set.";
            return;
        }
        else
        {
            SceneManager.LoadScene("Submarine");
        }
    }

    public void StartTwoPlayer()
    {
        if (!options.opt_set)
        {
            errorMsg.text = "Error: options not set.";
            return;
        }
        else
        {
            SceneManager.LoadScene("2Player");
        }
    }

    public void StartViewer()
    {
        if (!options.opt_set)
        {
            errorMsg.text = "Error: options not set.";
            return;
        }
        else
        {
            SceneManager.LoadScene("Viewer");
        }
    }

    public void GoToOptions()
    {
        errorMsg.text = string.Empty;
        this.gameObject.SetActive(false);
        options.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
