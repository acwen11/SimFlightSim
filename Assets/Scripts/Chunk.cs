using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
    public Vector3Int coord;

    public MeshGenerator meshgen;

    [HideInInspector]
    public Mesh[] mesh;
    public static GameObject[] surfaces;

    // MeshFilter meshFilter;
    // MeshRenderer meshRenderer;
    // MeshCollider meshCollider;
    bool generateCollider;

    public void DestroyOrDisable () {
        if (Application.isPlaying) {
            for (int ii = 0; ii < meshgen.set_num_surfaces; ii++)
            {
                mesh[ii].Clear ();
                Destroy(surfaces[ii]);
            }
            gameObject.SetActive (false);
        } else {
            for (int ii = 0; ii < meshgen.set_num_surfaces; ii++)
            {
                DestroyImmediate(surfaces[ii], false);
            }
            DestroyImmediate (gameObject, false);
        }
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp (Material mat, bool generateCollider) {
        Debug.Log("chunk sees" + meshgen.set_num_surfaces + "Surfaces");
        mesh = new Mesh[meshgen.set_num_surfaces];
        surfaces = new GameObject[meshgen.set_num_surfaces];

        MeshFilter[] filters = new MeshFilter[meshgen.set_num_surfaces];
        MeshRenderer[] renderers = new MeshRenderer[meshgen.set_num_surfaces];
        MeshCollider[] colliders = new MeshCollider[meshgen.set_num_surfaces];

        this.generateCollider = generateCollider;

        for (int ii = 0; ii < meshgen.set_num_surfaces; ii++)
        {
            if (surfaces[ii] == null)
            {
                surfaces[ii] = new GameObject("surf" + ii);
            }
            filters[ii] = surfaces[ii].GetComponent<MeshFilter> ();
            renderers[ii] = surfaces[ii].GetComponent<MeshRenderer> ();
            colliders[ii] = surfaces[ii].GetComponent<MeshCollider> ();

            if (filters[ii] == null) {
                filters[ii] = surfaces[ii].AddComponent<MeshFilter> ();
            }

            if (renderers[ii] == null) {
                renderers[ii] = surfaces[ii].AddComponent<MeshRenderer> ();
            }

            if (colliders[ii] == null && generateCollider) {
                colliders[ii] = surfaces[ii].AddComponent<MeshCollider> ();
            }
            if (colliders[ii] != null && !generateCollider) {
                DestroyImmediate (colliders[ii]);
            }
            mesh[ii] = filters[ii].sharedMesh;
            if (mesh[ii] == null)
            {
                mesh[ii] = new Mesh();
                mesh[ii].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                filters[ii].sharedMesh = mesh[ii];
            }

            if (generateCollider)
            {
                if (colliders[ii].sharedMesh == null)
                {
                    colliders[ii].sharedMesh = mesh[ii];
                }
                // force update
                colliders[ii].enabled = false;
                colliders[ii].enabled = true;
            }
            renderers[ii].material = mat;
        }

    }
}