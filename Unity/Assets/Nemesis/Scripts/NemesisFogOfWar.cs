using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class NemesisFogOfWar : MonoBehaviour
{
    [Range(3, 16)] public int visionRadius = 8;
    [Range(0.2f, 0.75f)] public float exploredDarkness = 0.48f;
    public Color fogColor = new Color(0.008f, 0.01f, 0.018f, 1f);

    private bool[,] walkable;
    private bool[,] explored;
    private bool[,] visible;
    private Vector2Int gridOrigin;
    private float cellWidth = 0.72f;
    private float cellHeight = 0.36f;
    private int lastPlayerX = int.MinValue;
    private int lastPlayerY = int.MinValue;
    private float nextRefresh;
    private Transform fogObject;
    private Mesh mesh;
    private Material material;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        EnsureMesh();
    }

    public void Configure(bool[,] grid, float width, float height, int radius, Vector2Int origin)
    {
        if (grid == null) return;

        walkable = grid;
        cellWidth = Mathf.Max(0.01f, width);
        cellHeight = Mathf.Max(0.01f, height);
        visionRadius = Mathf.Clamp(radius, 3, 16);
        gridOrigin = origin;
        explored = new bool[grid.GetLength(0), grid.GetLength(1)];
        visible = new bool[grid.GetLength(0), grid.GetLength(1)];
        lastPlayerX = int.MinValue;
        lastPlayerY = int.MinValue;

        EnsureMesh();
        RefreshVisibility(true);

        int visibleCount = 0;
        for (int x = 0; x < visible.GetLength(0); x++)
            for (int y = 0; y < visible.GetLength(1); y++)
                if (visible[x, y]) visibleCount++;

        Debug.Log("NEMESIS FOG: grid=" + grid.GetLength(0) + "x" + grid.GetLength(1) +
            ", visibleCells=" + visibleCount + ", radius=" + visionRadius +
            ", sharedVertices=true, shader=" + (material != null ? material.shader.name : "missing"));
    }

    void LateUpdate()
    {
        if (walkable == null || NemesisPlayer.Instance == null || Time.time < nextRefresh) return;

        nextRefresh = Time.time + 0.12f;
        Vector2Int cell = WorldToIndex(NemesisPlayer.Instance.transform.position);
        if (cell.x != lastPlayerX || cell.y != lastPlayerY)
            RefreshVisibility(false);
    }

    void EnsureMesh()
    {
        if (fogObject != null && mesh != null && meshRenderer != null) return;

        if (fogObject == null)
        {
            GameObject go = new GameObject("Fog of War - Soft Mask");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            fogObject = go.transform;
        }

        MeshFilter filter = fogObject.GetComponent<MeshFilter>();
        if (filter == null) filter = fogObject.gameObject.AddComponent<MeshFilter>();

        meshRenderer = fogObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = fogObject.gameObject.AddComponent<MeshRenderer>();

        mesh = new Mesh { name = "Nemesis Continuous Fog Mesh" };
        mesh.indexFormat = IndexFormat.UInt32;
        filter.sharedMesh = mesh;

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            material = new Material(shader)
            {
                name = "Nemesis Fog Material",
                color = Color.white,
                renderQueue = 3000
            };
            meshRenderer.sharedMaterial = material;
        }
        else
        {
            Debug.LogError("NEMESIS FOG: compatible transparent sprite shader is unavailable.");
        }

        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = 32000;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    void RefreshVisibility(bool force)
    {
        if (walkable == null || NemesisPlayer.Instance == null) return;

        Vector2Int origin = WorldToIndex(NemesisPlayer.Instance.transform.position);
        if (!Inside(origin)) origin = ClampToGrid(origin);

        if (!force && origin.x == lastPlayerX && origin.y == lastPlayerY) return;
        lastPlayerX = origin.x;
        lastPlayerY = origin.y;

        int width = walkable.GetLength(0);
        int height = walkable.GetLength(1);
        visible = new bool[width, height];

        int radius = visionRadius;
        int radiusSq = radius * radius;
        for (int x = Mathf.Max(0, origin.x - radius); x <= Mathf.Min(width - 1, origin.x + radius); x++)
        for (int y = Mathf.Max(0, origin.y - radius); y <= Mathf.Min(height - 1, origin.y + radius); y++)
        {
            int dx = x - origin.x;
            int dy = y - origin.y;
            if (dx * dx + dy * dy > radiusSq) continue;
            if (!walkable[x, y] && !HasWalkableNeighbour(x, y)) continue;
            if (!HasLineOfSight(origin, new Vector2Int(x, y))) continue;

            visible[x, y] = true;
            explored[x, y] = true;
        }

        RebuildFogMesh();
    }

    bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        int x = from.x;
        int y = from.y;
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        int sx = from.x < to.x ? 1 : -1;
        int sy = from.y < to.y ? 1 : -1;
        int error = dx - dy;

        while (true)
        {
            if (!(x == from.x && y == from.y) && !(x == to.x && y == to.y))
            {
                if (!Inside(new Vector2Int(x, y)) || !walkable[x, y])
                    return false;
            }

            if (x == to.x && y == to.y) return true;

            int twice = error * 2;
            if (twice > -dy) { error -= dy; x += sx; }
            if (twice < dx) { error += dx; y += sy; }
        }
    }

    bool HasWalkableNeighbour(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int xx = x + dx;
            int yy = y + dy;
            if (xx >= 0 && yy >= 0 && xx < walkable.GetLength(0) && yy < walkable.GetLength(1) && walkable[xx, yy])
                return true;
        }
        return false;
    }

    void RebuildFogMesh()
    {
        if (walkable == null || visible == null) return;
        EnsureMesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();
        Dictionary<Vector2Int, int> vertexIndices = new Dictionary<Vector2Int, int>();

        int width = walkable.GetLength(0);
        int height = walkable.GetLength(1);

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            int north = GetOrAddVertex(new Vector2Int(x, y + 1), vertexIndices, vertices, colors);
            int east = GetOrAddVertex(new Vector2Int(x + 1, y), vertexIndices, vertices, colors);
            int south = GetOrAddVertex(new Vector2Int(x, y - 1), vertexIndices, vertices, colors);
            int west = GetOrAddVertex(new Vector2Int(x - 1, y), vertexIndices, vertices, colors);

            triangles.Add(north); triangles.Add(east); triangles.Add(south);
            triangles.Add(north); triangles.Add(south); triangles.Add(west);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        meshRenderer.enabled = vertices.Count > 0 && material != null;
    }

    int GetOrAddVertex(Vector2Int localPoint, Dictionary<Vector2Int, int> indices, List<Vector3> vertices, List<Color> colors)
    {
        if (indices.TryGetValue(localPoint, out int existing))
            return existing;

        int globalX = localPoint.x + gridOrigin.x;
        int globalY = localPoint.y + gridOrigin.y;
        Vector3 worldPoint = new Vector3(
            (globalX - globalY) * cellWidth,
            (globalX + globalY) * cellHeight,
            0f
        );

        float alpha =
            CellFogAlpha(localPoint.x, localPoint.y - 1) +
            CellFogAlpha(localPoint.x - 1, localPoint.y) +
            CellFogAlpha(localPoint.x, localPoint.y + 1) +
            CellFogAlpha(localPoint.x + 1, localPoint.y);
        alpha *= 0.25f;

        int index = vertices.Count;
        indices.Add(localPoint, index);
        vertices.Add(worldPoint);
        colors.Add(new Color(fogColor.r, fogColor.g, fogColor.b, alpha));
        return index;
    }

    float CellFogAlpha(int x, int y)
    {
        if (!Inside(new Vector2Int(x, y))) return 1f;
        if (visible[x, y]) return 0f;
        return explored[x, y] ? exploredDarkness : 1f;
    }

    Vector2Int WorldToIndex(Vector2 world)
    {
        float u = world.x / cellWidth;
        float v = world.y / cellHeight;
        int gridX = Mathf.RoundToInt((u + v) * 0.5f);
        int gridY = Mathf.RoundToInt((v - u) * 0.5f);
        return new Vector2Int(gridX - gridOrigin.x, gridY - gridOrigin.y);
    }

    Vector2Int ClampToGrid(Vector2Int cell)
    {
        return new Vector2Int(
            Mathf.Clamp(cell.x, 0, walkable.GetLength(0) - 1),
            Mathf.Clamp(cell.y, 0, walkable.GetLength(1) - 1)
        );
    }

    bool Inside(Vector2Int point)
    {
        return walkable != null && point.x >= 0 && point.y >= 0 &&
            point.x < walkable.GetLength(0) && point.y < walkable.GetLength(1);
    }

    public bool IsVisible(Vector2 world)
    {
        if (visible == null) return true;
        Vector2Int point = WorldToIndex(world);
        return Inside(point) && visible[point.x, point.y];
    }

    void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
        if (material != null) Destroy(material);
    }
}

