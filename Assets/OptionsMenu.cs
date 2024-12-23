using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using UnityEditor.UI;

public class OptionsMenu : MonoBehaviour
{
    public TextMeshProUGUI isosurfCounter;
    public TextMeshProUGUI errorMsg;
    public MainMenu mainMenu;

    [HideInInspector]
    public bool opt_set;

    void Awake()
    {
        opt_set = false;        
    }

    public void setSimName(string name)
    {
        PlayerPrefs.SetString("simname", name);
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
        float minval = float.Parse(minstr);
        PlayerPrefs.SetFloat("min", minval);
    }
    public void setMax(string maxstr)
    {
        float maxval = float.Parse(maxstr);
        PlayerPrefs.SetFloat("max", maxval);
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

    public void TryBack()
    {
        // Check Options are Valid
        float minval = PlayerPrefs.GetFloat("min");
        float maxval = PlayerPrefs.GetFloat("max");
        int logscale = PlayerPrefs.GetInt("logscale");
        Debug.Log("In Options Menu, logscale = " + logscale);

        string sim_name = PlayerPrefs.GetString("simname");
        string simdir = @"Assets/Gridfunctions/" + sim_name;
        string simpar = @"Assets/Gridfunctions/" + sim_name + @"/" + sim_name + "_pars.txt";

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
            errorMsg.text = "Error: data directory not found.";
            return;
        }
        else if (!File.Exists(simpar))
        {
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
