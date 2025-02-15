using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;

public struct MassPars
{
    public MassPars(float mass, Vector3 coords)
    {
        gmass = mass;
        gcoords = coords;
    }

    public float gmass { get; }
    public Vector3 gcoords { get; }
}

public class ParReader : MonoBehaviour
{
    // From Parameter File
    [HideInInspector] public float par_bounds = 1f;
    [HideInInspector] public Vector3Int par_nChunks = Vector3Int.one;
    [HideInInspector] public int par_nSrcs = 1;
    [HideInInspector] public MassPars[] grav_masses;

    private void Awake()
    {
        string data_name = PlayerPrefs.GetString("simname");
        read_chunk_pars(@"Assets/Gridfunctions/" + data_name + @"/" + data_name + "_pars.txt", ref par_bounds, ref par_nChunks, ref par_nSrcs, ref grav_masses);
    }

    void read_chunk_pars(string par_path, ref float bd_size, ref Vector3Int nchunks, ref int nsrcs, ref MassPars[] masses)
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(par_path);
        bool bds_read = false;
        bool nchunks_read = false;
        nsrcs = 0;
        MassPars[] tmp_masses = new MassPars[5]; // Hard Coding max 5 masses
        string inp_txt = reader.ReadLine();
        // Debug.Log("Reading Line...");
        // Debug.Log(inp_txt);
        while ((!bds_read || !nchunks_read) && (inp_txt != null))
        {
            string[] inp_ln = inp_txt.Split();
            if (inp_ln[0] == "BoundsSize:")
            {
                bd_size = float.Parse(inp_ln[1], CultureInfo.InvariantCulture.NumberFormat);
                bds_read = true;
            }
            else if (inp_ln[0] == "NumChunks:")
            {
                // Must account for different Cactus/Unity coords
                nchunks = new Vector3Int(int.Parse(inp_ln[1]), int.Parse(inp_ln[3]), int.Parse(inp_ln[2]));
                nchunks_read = true;
            }
            else if (inp_ln[0] == "MassInfo:")
            {
                float tmpmass = float.Parse(inp_ln[1], CultureInfo.InvariantCulture.NumberFormat);
                // Must account for different Cactus/Unity coords
                Vector3 tmpcoords = new Vector3(float.Parse(inp_ln[2]), float.Parse(inp_ln[4]), float.Parse(inp_ln[3]));
                tmp_masses[nsrcs] = new MassPars(tmpmass, tmpcoords);
            }
            inp_txt = reader.ReadLine();
            // Debug.Log("Reading Line...");
            // Debug.Log(inp_txt);
        }

        if (nsrcs != 0)
        {
            masses = new MassPars[nsrcs];
            masses = tmp_masses;
        }

        if(!bds_read || !nchunks_read)
        {
            Debug.LogError("Cannot read chunk parameters.");
            // Quit Game
        }

        reader.Close();
    }
}
