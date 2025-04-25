using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;

public class HowtoPlayMenu : MonoBehaviour
{
    public MainMenu mainMenu;
    public GameObject firstButton;

    // Start is called before the first frame update
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
