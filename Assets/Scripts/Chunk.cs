using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {
    public Vector3Int coord;

    public MeshGenerator meshgen;

    [HideInInspector]
    public Mesh[] mesh;

    [HideInInspector]
    public GameObject[] surfaces;

    [HideInInspector]
    public bool surf_init;

    // MeshFilter meshFilter;
    // MeshRenderer meshRenderer;
    // MeshCollider meshCollider;
    bool generateCollider;

    private void Awake()
    {
        surf_init = false;
        mesh = new Mesh[meshgen.set_num_surfaces];
        surfaces = new GameObject[meshgen.set_num_surfaces];
    }

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
        surf_init = false;
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp (Material mat, bool generateCollider) {
        int ns = meshgen.set_num_surfaces;
        MeshFilter[] filters = new MeshFilter[ns];
        MeshRenderer[] renderers = new MeshRenderer[ns];
        MeshCollider[] colliders = new MeshCollider[ns];

        this.generateCollider = generateCollider;

        if (!surf_init)
        {
            for (int ii = 0; ii < ns; ii++)
            {
                surfaces[ii] = new GameObject("surf" + ii);
            }
            surf_init = true;
        }

        for (int ii = 0; ii < ns; ii++)
        {
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

            // Set Color
            float alp_pow = 0.5f;
            float ens = Mathf.Pow(ns, alp_pow);
            float alp = (ens - Mathf.Pow(ii, alp_pow)) / ens;
            Color iso_color = new Color(110, 0, 255, alp);
            Debug.Log(iso_color.ToString("F5"));
            renderers[ii].material = mat;
            renderers[ii].material.color = iso_color;
            //renderers[ii].material.color = Color.blue;
        }

    }
}