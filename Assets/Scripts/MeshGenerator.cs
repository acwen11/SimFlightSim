﻿using System.Collections;
using System.Collections.Generic;
// using System.Globalization;
using UnityEngine;
using Unity.Profiling;
using System.IO;
using Unity.Collections;
using Unity.IO.LowLevel.Unsafe;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour {

    const int threadGroupSize = 8;

    const int numPointsPerAxis = 128;

    // From Options Menu
    [HideInInspector] public string data_name;
    [HideInInspector] public int num_surfaces;
    [HideInInspector] public bool logscale;
    [HideInInspector] public float min_isoLevel;
    [HideInInspector] public float max_isoLevel;
    [HideInInspector] public Colormap cmap;

    // From Parameter File
    public ParReader par_reader;
    [HideInInspector] public float boundsSize = 1f;
    [HideInInspector] public Vector3Int numChunks = Vector3Int.one;

    [Header ("General Settings")]
    public DensityGenerator densityGenerator;

    public bool fixedMapSize;
    [ConditionalHide (nameof (fixedMapSize), false)]
    public Transform viewer;
    [ConditionalHide (nameof (fixedMapSize), false)]
    public float viewDistance = 30;

    [Space ()]
    public bool autoUpdateInEditor = true;
    public bool autoUpdateInGame = true;
    public ComputeShader shader;
    public Material trans_mat;
    public Material opq_mat;
    public bool generateColliders;

    [Header("Voxel Settings")]
    public Vector3 offset = Vector3.zero;

    [Header ("Gizmos")]
    public bool showBoundsGizmo = true;
    public Color boundsGizmoCol = Color.white;

    GameObject chunkHolder;
    const string chunkHolderName = "Chunks Holder";
    List<Chunk> chunks;
    Dictionary<Vector3Int, Chunk> existingChunks;
    Queue<Chunk> recycleableChunks;

    // Buffers
    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    bool settingsUpdated;

    // Profilers
    static readonly ProfilerMarker k_ChunkInit = new ProfilerMarker("Init Chunks");
    static readonly ProfilerMarker k_CreateMesh = new ProfilerMarker("Create Mesh");
    static readonly ProfilerMarker k_GenRho = new ProfilerMarker("Generate Rho");

    void Awake() {
        // Load Options
        data_name = PlayerPrefs.GetString("simname");
        num_surfaces = PlayerPrefs.GetInt("numSurfaces");
        logscale = PlayerPrefs.GetInt("logscale") != 0;
        min_isoLevel = PlayerPrefs.GetFloat("min");
        max_isoLevel = PlayerPrefs.GetFloat("max");
        /*
        Colormap prevcolor = GetComponent<Colormap>();
        while(prevcolor != null)
        {
            Destroy(prevcolor);
            prevcolor = GetComponent<Colormap>();
        }
        */
        cmap = gameObject.AddComponent<Colormap>();
        string cmapstr = PlayerPrefs.GetString("cmap");
        cmap.cmap = cmapstr;
    }

    private void Start()
    {
        // Get Parameters
        boundsSize = par_reader.par_bounds;
        numChunks = par_reader.par_nChunks;
        densityGenerator.datadir = data_name;

        if (Application.isPlaying && !fixedMapSize)
        {
            InitVariableChunkStructures();

            var oldChunks = FindObjectsOfType<Chunk>();
            for (int i = oldChunks.Length - 1; i >= 0; i--)
            {
                oldChunks[i].DestroyOrDisable();
                Destroy(oldChunks[i].gameObject);
            }
        }

        // THIS IS ASSUMED TO BE THE ONLY OPTION RN
        else if (Application.isPlaying && fixedMapSize)
        {
            CreateBuffers ();

            k_ChunkInit.Begin();
            InitChunks ();
            k_ChunkInit.End();

            UpdateAllChunks ();

            foreach (Chunk chunk in chunks) {
                chunk.RefreshCollider();
            }
        }
    }

    void Update () {
        // Update endless terrain
        if ((Application.isPlaying && !fixedMapSize)) {
            Run ();
        }

        if (settingsUpdated) {
            RequestMeshUpdate ();
            settingsUpdated = false;
        }
    }

    public void Run () {
        CreateBuffers ();

        if (!fixedMapSize)
        { 
            if (Application.isPlaying) {
                InitVisibleChunks ();
            }
        }

        // Release buffers immediately in editor
        if (!Application.isPlaying) {
            ReleaseBuffers ();
        }

    }

    /*
    void read_chunk_pars(string par_path, ref float bd_size, ref Vector3Int nchunks)
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(par_path);
        bool bds_read = false;
        bool nchunks_read = false;
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
            inp_txt = reader.ReadLine();
            // Debug.Log("Reading Line...");
            // Debug.Log(inp_txt);
        }
        if(!bds_read || !nchunks_read)
        {
            Debug.LogError("Cannot read chunk parameters.");
            // Quit Game
        }

        reader.Close();
    }
    */

    public void RequestMeshUpdate () {
        if ((Application.isPlaying && autoUpdateInGame) || (!Application.isPlaying && autoUpdateInEditor)) {
            Run ();
        }
    }

    void InitVariableChunkStructures () {
        recycleableChunks = new Queue<Chunk> ();
        chunks = new List<Chunk> ();
        existingChunks = new Dictionary<Vector3Int, Chunk> ();
    }

    void InitVisibleChunks () {
        if (chunks==null) {
            return;
        }
        CreateChunkHolder ();

        Vector3 p = viewer.position;
        Vector3 ps = p / boundsSize;
        Vector3Int viewerCoord = new Vector3Int (Mathf.RoundToInt (ps.x), Mathf.RoundToInt (ps.y), Mathf.RoundToInt (ps.z));

        int maxChunksInView = Mathf.CeilToInt (viewDistance / boundsSize);
        float sqrViewDistance = viewDistance * viewDistance;

        // Go through all existing chunks and flag for recyling if outside of max view dst
        for (int i = chunks.Count - 1; i >= 0; i--) {
            Chunk chunk = chunks[i];
            Vector3 centre = CentreFromCoord (chunk.coord);
            Vector3 viewerOffset = p - centre;
            Vector3 o = new Vector3 (Mathf.Abs (viewerOffset.x), Mathf.Abs (viewerOffset.y), Mathf.Abs (viewerOffset.z)) - Vector3.one * boundsSize / 2;
            float sqrDst = new Vector3 (Mathf.Max (o.x, 0), Mathf.Max (o.y, 0), Mathf.Max (o.z, 0)).sqrMagnitude;
            if (sqrDst > sqrViewDistance) {
                existingChunks.Remove (chunk.coord);
                recycleableChunks.Enqueue (chunk);
                chunks.RemoveAt (i);
            }
        }

        for (int x = -maxChunksInView; x <= maxChunksInView; x++) {
            for (int y = -maxChunksInView; y <= maxChunksInView; y++) {
                for (int z = -maxChunksInView; z <= maxChunksInView; z++) {
                    Vector3Int coord = new Vector3Int (x, y, z) + viewerCoord;

                    if (existingChunks.ContainsKey (coord)) {
                        continue;
                    }

                    Vector3 centre = CentreFromCoord (coord);
                    Vector3 viewerOffset = p - centre;
                    Vector3 o = new Vector3 (Mathf.Abs (viewerOffset.x), Mathf.Abs (viewerOffset.y), Mathf.Abs (viewerOffset.z)) - Vector3.one * boundsSize / 2;
                    float sqrDst = new Vector3 (Mathf.Max (o.x, 0), Mathf.Max (o.y, 0), Mathf.Max (o.z, 0)).sqrMagnitude;

                    // Chunk is within view distance and should be created (if it doesn't already exist)
                    if (sqrDst <= sqrViewDistance) {

                        Bounds bounds = new Bounds (CentreFromCoord (coord), Vector3.one * boundsSize);
                        if (IsVisibleFrom (bounds, Camera.main)) {
                            if (recycleableChunks.Count > 0) {
                                Chunk chunk = recycleableChunks.Dequeue ();
                                chunk.coord = coord;
                                existingChunks.Add (coord, chunk);
                                chunks.Add (chunk);
                                UpdateChunkMesh (chunk);
                            } else {
                                Chunk chunk = CreateChunk (coord);
                                chunk.coord = coord;
                                chunk.SetUp (trans_mat, opq_mat, generateColliders);
                                existingChunks.Add (coord, chunk);
                                chunks.Add (chunk);
                                UpdateChunkMesh (chunk);
                            }
                        }
                    }

                }
            }
        }
    }

    public bool IsVisibleFrom (Bounds bounds, Camera camera) {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes (camera);
        return GeometryUtility.TestPlanesAABB (planes, bounds);
    }

    //private unsafe NativeArray<float> Read_rho_arr(Vector3Int idx_ch)
    private unsafe float[] Read_rho_arr(Vector3Int idx_ch)
    {
        // Read in rho_b values from file
        // TODO: Cactus and Unity coords are inconsistent! Fix this in the data generation step instead.
        string dat_file = @"Assets/Gridfunctions/" + data_name + @"/" + data_name + "_" + idx_ch[0] + idx_ch[1] + idx_ch[2] + ".bin";
        int numpoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        NativeArray<float> rho_tmp = new NativeArray<float>(numpoints * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        float[] rho_out = new float[numpoints * 4];

        // Read in file
        Debug.Log("Chunk " + idx_ch + " reading chunk file " + dat_file);

        ReadCommand cmd;
        cmd.Offset = 0;
        cmd.Size = numpoints * 4 * sizeof(float);
        cmd.Buffer = rho_tmp.GetUnsafePtr();

        FileHandle fileHandle = AsyncReadManager.OpenFileAsync(dat_file);

        ReadCommandArray readCmdArray;
        readCmdArray.ReadCommands = &cmd;
        readCmdArray.CommandCount = 1;

        ReadHandle readHandle = AsyncReadManager.Read(fileHandle, readCmdArray);

        JobHandle closeJob = fileHandle.Close(readHandle.JobHandle);

        closeJob.Complete();

        rho_tmp.CopyTo(rho_out);

        readHandle.Dispose();

        for (int i = 0; i < readCmdArray.CommandCount; i++)
            UnsafeUtility.Free(readCmdArray.ReadCommands[i].Buffer, Allocator.Persistent);

        return rho_out;
    }

    float[] get_isosurface_vals()
    {
        float[] iso_vals = new float[num_surfaces];
        float tmpmin, tmpmax, drho;
        if (logscale)
        {
            tmpmin = Mathf.Log10(min_isoLevel);
            tmpmax = Mathf.Log10(max_isoLevel);
        }
        else
        {
            tmpmin = min_isoLevel;
            tmpmax = max_isoLevel;
        }
        drho = (tmpmax - tmpmin) / num_surfaces;
        
        for (int ii=0; ii<num_surfaces; ii++)
        {
            if (logscale)
            {
                iso_vals[ii] = Mathf.Pow(10, tmpmax - ii * drho);
            }
            else
            {
                iso_vals[ii] = tmpmax - ii * drho;
            }
        }

        return iso_vals;
    }

    public void UpdateChunkMesh (Chunk chunk) {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt (numVoxelsPerAxis / (float) threadGroupSize);
        float pointSpacing = boundsSize / (numPointsPerAxis - 1);

        Vector3Int coord = chunk.coord;
        Vector3 centre = CentreFromCoord (coord);

        Vector3 worldBounds = new Vector3 (numChunks.x, numChunks.y, numChunks.z) * boundsSize;

        float[] iso_levels = get_isosurface_vals();

        k_CreateMesh.Begin();
        for (int ii = 0; ii < num_surfaces; ii++)
        {
            k_GenRho.Begin();
            // densityGenerator.Generate(pointsBuffer, numPointsPerAxis, boundsSize, worldBounds, centre, offset, pointSpacing);
            pointsBuffer.SetData(Read_rho_arr(coord));
            // shader.SetFloats("points", Read_rho_arr(coord));
            k_GenRho.End();

            triangleBuffer.SetCounterValue(0);
            shader.SetBuffer(0, "points", pointsBuffer);
            shader.SetBuffer(0, "triangles", triangleBuffer);
            shader.SetInt("numPointsPerAxis", numPointsPerAxis);
            shader.SetFloat("isoLevel", iso_levels[ii]);

            shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

            // Get number of triangles in the triangle buffer
            ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
            int[] triCountArray = { 0 };
            triCountBuffer.GetData(triCountArray);
            int numTris = triCountArray[0];

            // Get triangle data from shader
            Triangle[] tris = new Triangle[numTris];
            triangleBuffer.GetData(tris, 0, 0, numTris);

            Mesh mesh = chunk.mesh[ii];
            mesh.Clear();

            var vertices = new Vector3[numTris * 3];
            var meshTriangles = new int[numTris * 3];

            for (int i = 0; i < numTris; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = tris[i][j];
                }
            }
            mesh.vertices = vertices;
            mesh.triangles = meshTriangles;

            mesh.RecalculateNormals();
        }
        k_CreateMesh.End();
    }

    public void UpdateAllChunks () {

        // Create mesh for each chunk
        foreach (Chunk chunk in chunks) {
            UpdateChunkMesh (chunk);
        }

    }

    void OnDestroy () {
        if (Application.isPlaying) {
            ReleaseBuffers ();
            Destroy(cmap);
        }
    }

    void CreateBuffers () {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
        // Otherwise, only create if null or if size has changed
        if (!Application.isPlaying || (pointsBuffer == null || numPoints != pointsBuffer.count)) {
            if (Application.isPlaying) {
                ReleaseBuffers ();
            }
            triangleBuffer = new ComputeBuffer (maxTriangleCount, sizeof (float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer (numPoints, sizeof (float) * 4);
            triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);

        }
    }

    void ReleaseBuffers () {
        if (triangleBuffer != null) {
            triangleBuffer.Release ();
            pointsBuffer.Release ();
            triCountBuffer.Release ();
        }
    }

    Vector3 CentreFromCoord (Vector3Int coord) {
        // Centre entire map at origin
        if (fixedMapSize) {
            Vector3 totalBounds = (Vector3) numChunks * boundsSize;
            return -totalBounds / 2 + (Vector3) coord * boundsSize + Vector3.one * boundsSize / 2;
        }

        return new Vector3 (coord.x, coord.y, coord.z) * boundsSize;
    }

    void CreateChunkHolder () {
        // Create/find mesh holder object for organizing chunks under in the hierarchy
        if (chunkHolder == null) {
            if (GameObject.Find (chunkHolderName)) {
                chunkHolder = GameObject.Find (chunkHolderName);
            } else {
                chunkHolder = new GameObject (chunkHolderName);
            }
        }
    }

    // Create/get references to all chunks
    void InitChunks () {
        CreateChunkHolder ();
        chunks = new List<Chunk> ();
        List<Chunk> oldChunks = new List<Chunk> (FindObjectsOfType<Chunk> ());

        // Go through all coords and create a chunk there if one doesn't already exist
        for (int x = 0; x < numChunks.x; x++) {
            for (int y = 0; y < numChunks.y; y++) {
                for (int z = 0; z < numChunks.z; z++) {
                    Vector3Int coord = new Vector3Int (x, y, z);
                    bool chunkAlreadyExists = false;

                    // If chunk already exists, add it to the chunks list, and remove from the old list.
                    for (int i = 0; i < oldChunks.Count; i++) {
                        if (oldChunks[i].coord == coord) {
                            chunks.Add (oldChunks[i]);
                            oldChunks.RemoveAt (i);
                            chunkAlreadyExists = true;
                            break;
                        }
                    }

                    // Create new chunk
                    if (!chunkAlreadyExists) {
                        var newChunk = CreateChunk (coord);
                        chunks.Add (newChunk);
                    }

                    chunks[chunks.Count - 1].SetUp (trans_mat, opq_mat, generateColliders);
                }
            }
        }

        // Delete all unused chunks
        for (int i = 0; i < oldChunks.Count; i++) {
            oldChunks[i].DestroyOrDisable ();
        }
    }

    Chunk CreateChunk (Vector3Int coord) {
        GameObject chunk = new GameObject ($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunk.transform.parent = chunkHolder.transform;
        Chunk newChunk = chunk.AddComponent<Chunk> ();
        newChunk.coord = coord;
        newChunk.num_surfaces = num_surfaces;
        newChunk.cmap = cmap;
        return newChunk;
    }

    void OnValidate() {
        settingsUpdated = true;
    }

    struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this [int i] {
            get {
                switch (i) {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }

    void OnDrawGizmos () {
        if (showBoundsGizmo) {
            Gizmos.color = boundsGizmoCol;

            List<Chunk> chunks = (this.chunks == null) ? new List<Chunk> (FindObjectsOfType<Chunk> ()) : this.chunks;
            foreach (var chunk in chunks) {
                Bounds bounds = new Bounds (CentreFromCoord (chunk.coord), Vector3.one * boundsSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube (CentreFromCoord (chunk.coord), Vector3.one * boundsSize);
            }
        }
    }

}