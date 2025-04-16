using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
//using UnityEditor.UI;

public class OptionsMenu : MonoBehaviour
{
    public TextMeshProUGUI isosurfCounter;
    public TextMeshProUGUI errorMsg;
    public MainMenu mainMenu;

    private TouchScreenKeyboard overlayKeyboard;
    public static string inputText = "";

    [HideInInspector]
    public bool opt_set;
    private string minvalstr;
    private string maxvalstr;
    private string simstr;

    void Awake()
    {
        opt_set = false;        
    }

    public void setSimName(string name)
    {
        simstr = name;
    }

    public void setCmap(int cmapid)
    {
        // TODO: define global enum?
        switch (cmapid)
        {
            case 1:
                PlayerPrefs.SetString("cmap", "inferno");
                break;
            case 2:
                PlayerPrefs.SetString("cmap", "magma");
                break;
            case 3:
                PlayerPrefs.SetString("cmap", "plasma");
                break;
            case 4:
                PlayerPrefs.SetString("cmap", "viridis");
                break;
            default:
                PlayerPrefs.SetString("cmap", "ERROR");
                break;
        }
    }

    public void setMin(string minstr)
    {
        //float minval = float.Parse(minstr);
        //PlayerPrefs.SetFloat("min", minval);
        minvalstr = minstr;
    }
    public void setMax(string maxstr)
    {
        //float maxval = float.Parse(maxstr);
        //PlayerPrefs.SetFloat("max", maxval);
        maxvalstr = maxstr;
    }
    public void setLog(bool log)
    {
        PlayerPrefs.SetInt("logscale", log ? 1 : 0);
    }
    
    public void setNumSurf(float ns)
    {
        int intns = Mathf.RoundToInt(ns);
        PlayerPrefs.SetInt("numSurfaces", intns);
        isosurfCounter.text = ns.ToString();
    }

    public void OpenKeyboard()
    {
        Debug.Log("Opening Keyboard");
        overlayKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

    public void TryBack()
    {
        // Check Options are Valid
        float minval = -99f;
        float maxval = -99f;
        try
        {
            Debug.Log("minstr = " + minvalstr);
            Debug.Log("maxstr = " + maxvalstr);
            minval = float.Parse(minvalstr);
            PlayerPrefs.SetFloat("min", minval);
            maxval = float.Parse(maxvalstr);
            PlayerPrefs.SetFloat("max", maxval);
        }
        catch
        {
            errorMsg.text = "Error: Cannot parse min and/or max values.";
            return;
        }
        //float minval = PlayerPrefs.GetFloat("min");
        //float maxval = PlayerPrefs.GetFloat("max");
        int logscale = PlayerPrefs.GetInt("logscale");

        string sim_name = PlayerPrefs.GetString("simname");
        //string simdir = @"Assets/SimData/" + sim_name;
        //string simpar = @"Assets/SimData/" + sim_name + @"/" + sim_name + "_pars.txt";
        string simdir = System.IO.Path.Combine(Application.streamingAssetsPath, "SimData", sim_name);
        string simpar = System.IO.Path.Combine(Application.streamingAssetsPath, "SimData", sim_name, sim_name + "_pars.txt");

        string setcmapstr = PlayerPrefs.GetString("cmap");

        if ((logscale == 1) && (minval <= 0))
        {
            errorMsg.text = "Error: minimum <= 0 when log scale is active.";
            return;
        }
        else if ((setcmapstr == "") || (setcmapstr == "ERROR"))
        {
            errorMsg.text = "Error: Colormap error.";
            return;
        }
        else if (minval >= maxval)
        {
            errorMsg.text = "Error: minimum value >= maximum value.";
            return;
        }
        else if (!Directory.Exists(simdir))
        {
            Debug.Log("Searched for data in " + simdir);
            errorMsg.text = "Error: data directory not found.";
            return;
        }
        else if (!File.Exists(simpar))
        {
            Debug.Log("Searched for data at " + simpar);
            errorMsg.text = "Error: chunk parameters not found.";
            return;
        }
        else
        {
            opt_set = true;
            errorMsg.text = string.Empty;
            this.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(true);
            return;
        }
    }

}
