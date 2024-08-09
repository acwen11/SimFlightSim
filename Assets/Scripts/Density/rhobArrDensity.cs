using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class rhobArrDensity : DensityGenerator {

    float[] Read_rhob_ascii(int numpoints)
    {
        float[] rhob = new float[numpoints];

        // Read in file
        // Debug.Log("My file:" + File.Exists(@"C:\Users\Allen\CactusAstronaut\Assets\Gridfunctions\rhob_vis139.asc"));
        var asc_input = File.ReadAllLines(@"C:\Users\Allen\CactusAstronaut\Assets\Gridfunctions\rhob_vis139.asc");

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
        // Read in rho_b values from file
        float[] rhob = Read_rhob_ascii(numPointsPerAxis * numPointsPerAxis * numPointsPerAxis);
        var rhobBuffer = new ComputeBuffer (rhob.Length, sizeof (float));

        // Send rhob to shader (runs on GPU?)
        rhobBuffer.SetData (rhob);
        buffersToRelease = new List<ComputeBuffer>();
        buffersToRelease.Add(rhobBuffer);
        densityShader.SetBuffer(0, "rhob", rhobBuffer);

        return base.Generate (pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, spacing);
    }
}