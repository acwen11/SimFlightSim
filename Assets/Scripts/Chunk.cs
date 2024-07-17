using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
    public Vector3Int coord;

    public MeshGenerator meshgen;

    [HideInInspector]
    public Mesh[] mesh;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    bool generateCollider;

    public void DestroyOrDisable () {
        if (Application.isPlaying) {
            for (int ii = 0; ii < MeshGenerator.num_surfaces; ii++)
            {
                mesh[ii].Clear ();
            }
            gameObject.SetActive (false);
        } else {
            DestroyImmediate (gameObject, false);
        }
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp (Material mat, bool generateCollider) {
        Debug.Log("chunk sees" + meshgen.set_num_surfaces + "Surfaces");
        mesh = new Mesh[meshgen.set_num_surfaces];

        this.generateCollider = generateCollider;

        meshFilter = GetComponent<MeshFilter> ();
        meshRenderer = GetComponent<MeshRenderer> ();
        meshCollider = GetComponent<MeshCollider> ();

        if (meshFilter == null) {
            meshFilter = gameObject.AddComponent<MeshFilter> ();
        }

        if (meshRenderer == null) {
            meshRenderer = gameObject.AddComponent<MeshRenderer> ();
        }

        if (meshCollider == null && generateCollider) {
            meshCollider = gameObject.AddComponent<MeshCollider> ();
        }
        if (meshCollider != null && !generateCollider) {
            DestroyImmediate (meshCollider);
        }

        for (int ii = 0; ii < MeshGenerator.num_surfaces; ii++)
        {
            mesh[ii] = meshFilter.sharedMesh;
            if (mesh[ii] == null)
            {
                mesh[ii] = new Mesh();
                mesh[ii].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilter.sharedMesh = mesh[ii];
            }

            if (generateCollider)
            {
                if (meshCollider.sharedMesh == null)
                {
                    meshCollider.sharedMesh = mesh[ii];
                }
                // force update
                meshCollider.enabled = false;
                meshCollider.enabled = true;
            }
        }

        meshRenderer.material = mat;
    }
}