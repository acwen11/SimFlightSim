using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rhobArrDensity : DensityGenerator {

    float[] Read_rhob_ascii(int numpoints)
    {
        float[] rhob = new float[numpoints];

        // Read in file

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

        return base.Generate (pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, spacing);
    }
}