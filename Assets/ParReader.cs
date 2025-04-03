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

public struct AHPars
{
    public AHPars(float radius, Vector3 coords)
    {
        grad = radius;
        gcoords = coords;
    }

    public float grad { get; }
    public Vector3 gcoords { get; }
}

public class ParReader : MonoBehaviour
{
    // From Parameter File
    [HideInInspector] public float par_bounds = 1f;
    [HideInInspector] public Vector3Int par_nChunks = Vector3Int.one;
    [HideInInspector] public int par_nSrcs = 0;
    [HideInInspector] public MassPars[] grav_masses;
    [HideInInspector] public int par_nAHs = 0;
    [HideInInspector] public AHPars[] ah_list;

    private void Awake()
    {
        string data_name = PlayerPrefs.GetString("simname");
        read_chunk_pars(@"Assets/Gridfunctions/" + data_name + @"/" + data_name + "_pars.txt", ref par_bounds, ref par_nChunks,
            ref par_nSrcs, ref par_nAHs, ref grav_masses, ref ah_list);
        Debug.Log("Read " + par_nSrcs + " Masses.");
    }

    private void Start()
    {
        // TODO: Draw AHs. Spheres for now, eventually import VTK files.
    }

    void read_chunk_pars(string par_path, ref float bd_size, ref Vector3Int nchunks, ref int nsrcs, ref int nbhs, ref MassPars[] masses, ref AHPars[] ahs)
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(par_path);
        bool bds_read = false;
        bool nchunks_read = false;
        nsrcs = 0;
        nbhs = 0;
        MassPars[] tmp_masses = new MassPars[5]; // Hard Coding max 5 masses
        AHPars[] tmp_ahs = new AHPars[5]; // Hard Coding max 5 apparent horizons
        string inp_txt = reader.ReadLine();
        // Debug.Log("Reading Line...");
        // Debug.Log(inp_txt);
        // while ((!bds_read || !nchunks_read) && (inp_txt != null)) TODO: better param check at end
        while (inp_txt != null)
        {
            string[] inp_ln = inp_txt.Split();
            Debug.Log("Detecting info for " + inp_ln[0]);
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
            else if (inp_ln[0] == "NSInfo:")
            {
                Debug.Log("Reading NS Info.");
                float tmpmass = float.Parse(inp_ln[1], CultureInfo.InvariantCulture.NumberFormat);
                Debug.Log("Read NS mass = " + tmpmass);
                // Must account for different Cactus/Unity coords
                Vector3 tmpcoords = new Vector3(float.Parse(inp_ln[2]), float.Parse(inp_ln[4]), float.Parse(inp_ln[3]));
                tmp_masses[nsrcs] = new MassPars(tmpmass, tmpcoords);
                nsrcs += 1;
            }
            else if (inp_ln[0] == "BHInfo:")
            {
                float tmpmass = float.Parse(inp_ln[1], CultureInfo.InvariantCulture.NumberFormat);
                float tmprad = float.Parse(inp_ln[2], CultureInfo.InvariantCulture.NumberFormat);
                // Must account for different Cactus/Unity coords
                Vector3 tmpcoords = new Vector3(float.Parse(inp_ln[3]), float.Parse(inp_ln[5]), float.Parse(inp_ln[4]));
                tmp_masses[nsrcs] = new MassPars(tmpmass, tmpcoords);
                nsrcs += 1;
                tmp_ahs[nbhs] = new AHPars(tmprad, tmpcoords);
                nbhs += 1;
            }

            inp_txt = reader.ReadLine();
            // Debug.Log("Reading Line...");
            // Debug.Log(inp_txt);
        }

        if (nsrcs != 0)
        {
            masses = new MassPars[nsrcs];
            for (int ii=0; ii<nsrcs; ii++)
            {
                masses[ii] = tmp_masses[ii];
            }
        }

        if (nbhs != 0)
        {
            ahs = new AHPars[nbhs];
            for (int ii=0; ii<nbhs; ii++)
            {
                ahs[ii] = tmp_ahs[ii];
            }
        }

        if(!bds_read || !nchunks_read)
        {
            Debug.LogError("Cannot read chunk parameters.");
            // Quit Game
        }

        reader.Close();
    }
}
