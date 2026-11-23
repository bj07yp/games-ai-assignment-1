using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// CET2000 Lab 08 - builds ("bakes") the NavMesh when you press Play, and draws it (provided, no changes needed).
//
// In your own projects you bake with the AI Navigation package: add a NavMesh Surface component and click Bake.
// This script uses the same engine (UnityEngine.AI.NavMeshBuilder) from code, so this lab needs no extra package,
// and you can change the agent radius and bake again while the game is running.
//
// What is baked: every collider inside the bake area, except objects on the "Ignore Raycast" layer
// (the agents, the door and the waypoint markers). The door is a NavMeshObstacle that carves its own hole instead.
[DefaultExecutionOrder(-100)]
public class NavMeshBaker : MonoBehaviour
{
    [Tooltip("How far the walkable area stays away from walls: the radius of the agent the NavMesh is built for.")]
    [Range(0.1f, 1.5f)] public float agentRadius = 0.5f;
    public Vector3 bakeSize = new Vector3(30f, 10f, 24f);
    [Tooltip("Agents start disabled (there is no NavMesh until Play). They are switched on after the bake.")]
    public NavMeshAgent[] agentsToEnable;
    public bool showNavMesh = true;
    public Color fillColour = new Color(0.62f, 0.84f, 0.95f);
    public Color edgeColour = new Color(0.05f, 0.42f, 0.58f);

    public int TriangleCount { get; private set; }
    public float BakedRadius { get; private set; }

    NavMeshDataInstance instance;
    MeshFilter fill, edges;
    float nextRefresh;

    void Awake()
    {
        Bake();
        foreach (NavMeshAgent a in agentsToEnable)
            if (a != null) a.enabled = true;
        fill = MakeLayer("NavMesh view (fill)", fillColour, 0.02f);
        edges = MakeLayer("NavMesh view (edges)", edgeColour, 0.03f);
        RefreshView();
    }

    public void Bake()
    {
        var sources = new List<NavMeshBuildSource>();
        var markups = new List<NavMeshBuildMarkup>();
        int mask = ~(1 << 2);                                   // every layer except Ignore Raycast
        var bounds = new Bounds(transform.position, bakeSize);
        NavMeshBuilder.CollectSources(bounds, mask, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);

        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);   // the default "Humanoid" agent type
        settings.agentRadius = agentRadius;
        NavMeshData data = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);

        if (instance.valid) NavMesh.RemoveNavMeshData(instance);
        instance = NavMesh.AddNavMeshData(data);
        BakedRadius = agentRadius;

        foreach (NavMeshAgent a in agentsToEnable)               // put running agents back on the new NavMesh
            if (a != null && a.enabled) a.Warp(a.transform.position);
        nextRefresh = 0f;
        Debug.Log("[Lab 08] NavMesh baked with agent radius " + agentRadius.ToString("0.00"));
    }

    void Update()
    {
        if (fill == null) return;
        fill.gameObject.SetActive(showNavMesh);
        edges.gameObject.SetActive(showNavMesh);
        if (showNavMesh && Time.time >= nextRefresh)
        {
            nextRefresh = Time.time + 0.5f;                      // also picks up holes carved by the door
            RefreshView();
        }
    }

    MeshFilter MakeLayer(string name, Color colour, float height)
    {
        var go = new GameObject(name);
        go.layer = 2;
        go.transform.position = new Vector3(0f, height, 0f);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = LabDraw.Unlit(colour);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mf.sharedMesh = new Mesh();
        return mf;
    }

    void RefreshView()
    {
        if (fill == null) return;
        NavMeshTriangulation t = NavMesh.CalculateTriangulation();
        int[] tri = t.indices;
        TriangleCount = tri.Length / 3;

        // Fill: every triangle twice (both windings), so it is visible from above whichever way it faces.
        var both = new int[tri.Length * 2];
        for (int i = 0; i < tri.Length; i += 3)
        {
            both[i] = tri[i]; both[i + 1] = tri[i + 1]; both[i + 2] = tri[i + 2];
            both[tri.Length + i] = tri[i]; both[tri.Length + i + 1] = tri[i + 2]; both[tri.Length + i + 2] = tri[i + 1];
        }
        // Edges: the three sides of every triangle.
        var lines = new int[tri.Length * 2];
        for (int i = 0; i < tri.Length; i += 3)
        {
            int k = i * 2;
            lines[k] = tri[i]; lines[k + 1] = tri[i + 1];
            lines[k + 2] = tri[i + 1]; lines[k + 3] = tri[i + 2];
            lines[k + 4] = tri[i + 2]; lines[k + 5] = tri[i];
        }
        SetMesh(fill.sharedMesh, t.vertices, both, MeshTopology.Triangles);
        SetMesh(edges.sharedMesh, t.vertices, lines, MeshTopology.Lines);
    }

    static void SetMesh(Mesh m, Vector3[] v, int[] idx, MeshTopology topology)
    {
        m.Clear();
        m.indexFormat = v.Length > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        m.vertices = v;
        m.SetIndices(idx, topology, 0);
        m.RecalculateBounds();
    }

    void OnDestroy()
    {
        if (instance.valid) NavMesh.RemoveNavMeshData(instance);
    }
}
