using UnityEngine;
using UnityEngine.Rendering;

// CET2000 Lab 08 - drawing helpers (provided, no changes needed).
// Makes materials that work in both URP and the Built-in Render Pipeline, and draws arrows and circles
// on the floor with LineRenderers.
public static class LabDraw
{
    public static bool URP { get { return GraphicsSettings.currentRenderPipeline != null; } }

    public static Shader LitShader()
    {
        Shader s = URP ? Shader.Find("Universal Render Pipeline/Lit") : Shader.Find("Standard");
        return s != null ? s : Shader.Find("Sprites/Default");
    }

    public static Shader UnlitShader()
    {
        Shader s = URP ? Shader.Find("Universal Render Pipeline/Unlit") : Shader.Find("Unlit/Color");
        return s != null ? s : Shader.Find("Sprites/Default");
    }

    public static Material Lit(Color c) { var m = new Material(LitShader()); SetColour(m, c); return m; }
    public static Material Unlit(Color c) { var m = new Material(UnlitShader()); SetColour(m, c); return m; }

    public static void SetColour(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }

    public static Color GetColour(Material m)
    {
        if (m.HasProperty("_BaseColor")) return m.GetColor("_BaseColor");
        if (m.HasProperty("_Color")) return m.GetColor("_Color");
        return Color.grey;
    }

    // A material made for the other pipeline draws pink. Swap any such material for one that works, keeping its colour.
    public static void FixMaterials()
    {
        foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r is LineRenderer || r is TrailRenderer) continue;
            Material m = r.sharedMaterial;
            if (m != null && !WrongShader(m.shader)) continue;
            r.sharedMaterial = Lit(m != null ? GetColour(m) : Color.grey);
        }
    }

    static bool WrongShader(Shader s)
    {
        if (s == null || !s.isSupported) return true;
        bool urpShader = s.name.StartsWith("Universal Render Pipeline");
        return URP ? s.name == "Standard" : urpShader;
    }

    public static LineRenderer Line(string name, Color c, float width, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial = Unlit(c);
        lr.startWidth = width;
        lr.endWidth = width;
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.numCapVertices = 2;
        lr.numCornerVertices = 2;
        lr.shadowCastingMode = ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    // An arrow on the floor from 'from' along 'vec' (y is ignored).
    public static void Arrow(LineRenderer lr, Vector3 from, Vector3 vec, float y = 0.15f)
    {
        vec.y = 0f;
        if (vec.sqrMagnitude < 0.0004f) { lr.positionCount = 0; return; }
        from.y = y;
        Vector3 to = from + vec;
        float head = Mathf.Min(0.4f, vec.magnitude * 0.4f);
        Vector3 back = -vec.normalized * head;
        Vector3 side = Vector3.Cross(Vector3.up, back).normalized * head * 0.6f;
        lr.loop = false;
        lr.positionCount = 5;
        lr.SetPositions(new[] { from, to, to + back + side, to, to + back - side });
    }

    public static void Circle(LineRenderer lr, Vector3 centre, float radius, float y = 0.06f, int segments = 60)
    {
        var pts = new Vector3[segments];
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            pts[i] = new Vector3(centre.x + Mathf.Cos(a) * radius, y, centre.z + Mathf.Sin(a) * radius);
        }
        lr.loop = true;
        lr.positionCount = segments;
        lr.SetPositions(pts);
    }

    public static void Hide(LineRenderer lr) { if (lr != null) lr.positionCount = 0; }

    // Mouse position (from an OnGUI event) to a point on the floor. Returns false if the click missed.
    public static bool MouseToFloor(Vector2 guiMouse, out Vector3 point)
    {
        point = Vector3.zero;
        Camera cam = Camera.main;
        if (cam == null) return false;
        Ray ray = cam.ScreenPointToRay(new Vector3(guiMouse.x, Screen.height - guiMouse.y, 0f));
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 500f)) return false;     // agents, door and markers are on Ignore Raycast
        point = hit.point;
        point.y = 0f;
        return true;
    }
}
