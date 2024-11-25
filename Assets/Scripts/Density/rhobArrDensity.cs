using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class rhobArrDensity : DensityGenerator {
    float[] Read_rhob_ascii(string dat_file, int numpoints)
    {
        float[] rhob = new float[numpoints];

        // Read in file
        // Debug.Log("My file:" + File.Exists(@"C:\Users\Allen\CactusAstronaut\Assets\Gridfunctions\rhob_vis139.asc"));
        var asc_input = File.ReadAllLines(dat_file);

        // Create a list we can add the numbers we're parsing to. 
        List<float> parsedNumbers = new List<float>() ;

        for (int i = 0; i < asc_input.Length; i++)
        {
            // Check if the current number is an empty line
            if (string.IsNullOrEmpty(asc_input[i]))
            {
                continue;
            }
            // If not, try to convert the value to a float
            if (float.TryParse(asc_input[i], out float parsedValue))
            {
                // If the conversion was successful, add it to the parsed float list 
                parsedNumbers.Add(parsedValue);
            }
        }

        // If input file is valid, fill rhob array
        Debug.Assert(numpoints == parsedNumbers.Count, "# rho_b data points != numpoints.");
        for (int ii = 0; ii < numpoints; ii++)
        {
            rhob[ii] = parsedNumbers[ii];
        }

        return rhob;
    }

    public override ComputeBuffer Generate (ComputeBuffer pointsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, Vector3 centre, Vector3 offset, float spacing) {
        // Recover chunk idx
        Vector3Int idx_ch = Vector3Int.FloorToInt((centre + (worldBounds / 2) - (Vector3.one * boundsSize / 2)) / (boundsSize));

        // Read in rho_b values from file
        // TODO: Cactus and Unity coords are inconsistent! Fix this in the data generation step instead.
        string datfile = @"Assets/Gridfunctions/" + datadir + @"/" + datadir + "_" + idx_ch[2] + idx_ch[0] + idx_ch[1] + ".txt";
        Debug.Log("Reading chunk file " + datfile);
        Vector3Int grid_dim = Vector3Int.FloorToInt(worldBounds / boundsSize) * numPointsPerAxis;
        float[] rhob = Read_rhob_ascii(datfile, numPointsPerAxis * numPointsPerAxis * numPointsPerAxis);
        // float[] rhob = new float[numPointsPerAxis * numPointsPerAxis * numPointsPerAxis];

        // Loop over array
        var rhobBuffer = new ComputeBuffer (rhob.Length, sizeof (float));
        // 
        // for (int kk = 0; kk < numPointsPerAxis; kk++)
        // {
        //     for (int jj = 0; jj < numPointsPerAxis; jj++)
        //     {
        //         for (int ii = 0; ii < numPointsPerAxis; ii++)
        //         {
        //             int ridx = ii + numPointsPerAxis * jj + numPointsPerAxis * numPointsPerAxis * kk;
        //             int GFi = ii + idx_ch[1] * numPointsPerAxis; // Note that Unity swaps x and y
        //             int GFj = jj + idx_ch[0] * numPointsPerAxis;
        //             int GFk = kk + idx_ch[2] * numPointsPerAxis;
        //             int GFidx = GFi + grid_dim[0] * GFj + grid_dim[0] * grid_dim[1] * GFk;

        //             rhob[ridx] = rhoGF[GFidx];
        //         }
        //     }
        // }

        // Send rhob to shader (runs on GPU?)
        rhobBuffer.SetData (rhob);
        buffersToRelease = new List<ComputeBuffer>();
        buffersToRelease.Add(rhobBuffer);
        densityShader.SetBuffer(0, "rhob", rhobBuffer);

        return base.Generate (pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, spacing);
    }
}