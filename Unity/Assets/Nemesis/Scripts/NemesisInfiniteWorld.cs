using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class NemesisInfiniteWorld : MonoBehaviour
{
    [Header("Streaming")]
    public Transform target;
    [Range(12, 32)] public int chunkSize = 20;
    [Range(1, 4)] public int chunkRadius = 2;
    public int worldSeed = 721904;
    [Header("Isometric grid")]
    public float cellWidth = 0.72f;
    public float cellHeight = 0.36f;

    private sealed class TileBatch
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<int> triangles = new List<int>();
    }

    private sealed class LoadedChunk
    {
        public GameObject root;
        public readonly List<Mesh> meshes = new List<Mesh>();
    }

    private readonly Dictionary<Vector2Int, LoadedChunk> loaded = new Dictionary<Vector2Int, LoadedChunk>();
    private readonly Dictionary<int, Material> materials = new Dictionary<int, Material>();
    private Sprite[] terrain = new Sprite[4];
    private Sprite[] props = new Sprite[6];
    private Sprite[] enemies = new Sprite[4];
    private Vector2Int lastChunk = new Vector2Int(int.MinValue, int.MinValue);
    private bool assetsReady;

    private void Start()
    {
        if (target == null && NemesisPlayer.Instance != null)
            target = NemesisPlayer.Instance.transform;

        LoadRuntimeAssets();
        UpdateStreaming(true);
    }

    private void Update()
    {
        if (!assetsReady) return;
        if (target == null && NemesisPlayer.Instance != null)
            target = NemesisPlayer.Instance.transform;
        if (target == null) return;

        Vector2Int cell = WorldToCell(target.position);
        Vector2Int chunk = CellToChunk(cell);
        if (chunk != lastChunk)
            UpdateStreaming(false);
    }

    private void OnDestroy()
    {
        foreach (LoadedChunk chunk in loaded.Values)
        {
            if (chunk.root != null) Destroy(chunk.root);
            foreach (Mesh mesh in chunk.meshes)
                if (mesh != null) Destroy(mesh);
        }
        loaded.Clear();
        foreach (Material material in materials.Values)
            if (material != null) Destroy(material);
        materials.Clear();
    }

    private void LoadRuntimeAssets()
    {
        terrain[0] = Resources.Load<Sprite>("NemesisWorld/Terrain/Cobble");
        terrain[1] = Resources.Load<Sprite>("NemesisWorld/Terrain/Cracked");
        terrain[2] = Resources.Load<Sprite>("NemesisWorld/Terrain/Stone");
        terrain[3] = Resources.Load<Sprite>("NemesisWorld/Terrain/Lava");
        props[0] = Resources.Load<Sprite>("NemesisWorld/Props/OakTree");
        props[1] = Resources.Load<Sprite>("NemesisWorld/Props/PineTree");
        props[2] = Resources.Load<Sprite>("NemesisWorld/Props/DeadTree");
        props[3] = Resources.Load<Sprite>("NemesisWorld/Props/Bush");
        props[4] = Resources.Load<Sprite>("NemesisWorld/Props/Boulder");
        props[5] = Resources.Load<Sprite>("NemesisWorld/Props/Tomb");
        enemies[0] = Resources.Load<Sprite>("NemesisWorld/Enemies/Specter");
        enemies[1] = Resources.Load<Sprite>("NemesisWorld/Enemies/Revenant");
        enemies[2] = Resources.Load<Sprite>("NemesisWorld/Enemies/Bat");
        enemies[3] = Resources.Load<Sprite>("NemesisWorld/Enemies/Gargoyle");

        assetsReady = terrain[0] != null && terrain[1] != null && terrain[2] != null;
        if (!assetsReady)
            Debug.LogError("NEMESIS INFINITE WORLD: runtime sprite resources are missing. Run Nemesis/Install Infinite World Runtime Assets in the Editor.");
    }

    private void UpdateStreaming(bool force)
    {
        if (target == null || !assetsReady) return;
        Vector2Int centerCell = WorldToCell(target.position);
        Vector2Int centerChunk = CellToChunk(centerCell);
        lastChunk = centerChunk;

        HashSet<Vector2Int> required = new HashSet<Vector2Int>();
        for (int x = -chunkRadius; x <= chunkRadius; x++)
        for (int y = -chunkRadius; y <= chunkRadius; y++)
        {
            Vector2Int key = centerChunk + new Vector2Int(x, y);
            required.Add(key);
            if (!loaded.ContainsKey(key))
                loaded.Add(key, GenerateChunk(key));
        }

        List<Vector2Int> remove = new List<Vector2Int>();
        foreach (Vector2Int key in loaded.Keys)
            if (!required.Contains(key)) remove.Add(key);

        foreach (Vector2Int key in remove)
        {
            LoadedChunk chunk = loaded[key];
            if (chunk.root != null) Destroy(chunk.root);
            foreach (Mesh mesh in chunk.meshes)
                if (mesh != null) Destroy(mesh);
            loaded.Remove(key);
        }
    }

    private LoadedChunk GenerateChunk(Vector2Int key)
    {
        LoadedChunk result = new LoadedChunk();
        result.root = new GameObject("Infinite Chunk [" + key.x + ", " + key.y + "]");
        result.root.transform.SetParent(transform, false);

        Vector2Int firstCell = key * chunkSize;
        Vector3 origin = Iso(firstCell.x, firstCell.y);
        result.root.transform.position = origin;

        Dictionary<int, TileBatch> batches = new Dictionary<int, TileBatch>();
        List<Vector2Int> enemyCells = new List<Vector2Int>();

        for (int y = 0; y < chunkSize; y++)
        for (int x = 0; x < chunkSize; x++)
        {
            int gx = firstCell.x + x;
            int gy = firstCell.y + y;

            // Keep the hand-authored starting region intact. New terrain begins exactly beyond its edge.
            if (IsHandAuthoredCell(gx, gy)) continue;

            Vector2Int cell = new Vector2Int(gx, gy);
            int seed = CellHash(gx, gy);
            bool path = IsRoad(gx, gy);
            Sprite tile = PickGround(gx, gy, path);
            int tileIndex = Array.IndexOf(terrain, tile);
            if (tile != null)
            {
                if (!batches.TryGetValue(tileIndex, out TileBatch batch))
                {
                    batch = new TileBatch();
                    batches.Add(tileIndex, batch);
                }
                AddDiamond(batch, tile, Iso(gx, gy) - origin);
            }

            if (!path && tileIndex >= 0 && Hash01(gx, gy, 31) < 0.012f)
                SpawnWorldProp(result.root.transform, cell, seed);
        }

        foreach (KeyValuePair<int, TileBatch> entry in batches)
            BuildTileMesh(result, entry.Key, entry.Value);

        int centerX = firstCell.x + chunkSize / 2;
        int centerY = firstCell.y + chunkSize / 2;
        bool beyondStart = Mathf.Abs(centerX) > 18 || Mathf.Abs(centerY) > 14;
        if (beyondStart && Hash01(centerX, centerY, 71) > 0.84f)
            SpawnChunkEnemies(result.root.transform, key, firstCell);

        return result;
    }

    private Sprite PickGround(int x, int y, bool path)
    {
        if (path) return terrain[0];
        float biome = Mathf.PerlinNoise((x + worldSeed) * 0.031f, (y - worldSeed) * 0.031f);
        float heat = Mathf.PerlinNoise((x - worldSeed * 2) * 0.045f, (y + worldSeed) * 0.045f);
        if (heat > 0.91f && terrain[3] != null) return terrain[3];
        if (biome > 0.67f) return terrain[2];
        if (biome > 0.42f) return terrain[1];
        return terrain[0];
    }

    private bool IsRoad(int x, int y)
    {
        // The first route continues beyond the cathedral courtyard and bends through later chunks.
        float mainRoad = Mathf.Sin(x * 0.035f) * 2.8f + x * 0.56f + 2.1f;
        bool forward = x > 12 && Mathf.Abs(y - mainRoad) < 1.35f;
        bool southernTrail = y < -10 && Mathf.Abs(x + 1f + Mathf.Sin(y * 0.06f) * 2.0f) < 1.2f;
        bool westernTrail = x < -14 && Mathf.Abs(y - (Mathf.Sin(x * 0.045f) * 3f - 1f)) < 1.2f;
        return forward || southernTrail || westernTrail;
    }

    private void SpawnWorldProp(Transform parent, Vector2Int cell, int seed)
    {
        int kind = Mathf.Abs(seed % props.Length);
        Sprite sprite = props[kind];
        if (sprite == null) return;

        Vector3 p = Iso(cell.x, cell.y);
        GameObject go = new GameObject("World Prop - " + sprite.name);
        go.transform.SetParent(parent, false);
        go.transform.position = p;
        float scale = kind == 0 || kind == 1 || kind == 2 ? 1.15f : kind == 5 ? 0.95f : 1f;
        go.transform.localScale = new Vector3(scale, scale, 1f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        NemesisDepthSort sort = go.AddComponent<NemesisDepthSort>();
        sort.baseOrder = 100;

        if (kind == 0 || kind == 1 || kind == 2 || kind == 4 || kind == 5)
        {
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.45f, 0.28f);
            collider.offset = new Vector2(0f, 0.14f);
        }
    }

    private void SpawnChunkEnemies(Transform parent, Vector2Int key, Vector2Int firstCell)
    {
        int amount = Hash01(key.x * 31 + 5, key.y * 17 - 3, 151) > 0.94f ? 2 : 1;
        for (int i = 0; i < amount; i++)
        {
            int gx = firstCell.x + 4 + Mathf.Abs(CellHash(key.x * 13 + i, key.y * 7 - i)) % Mathf.Max(1, chunkSize - 8);
            int gy = firstCell.y + 4 + Mathf.Abs(CellHash(key.x * 5 - i, key.y * 19 + i)) % Mathf.Max(1, chunkSize - 8);
            if (IsHandAuthoredCell(gx, gy) || IsRoad(gx, gy)) continue;
            Sprite sprite = enemies[Mathf.Abs(CellHash(gx, gy) % enemies.Length)];
            if (sprite == null) continue;

            GameObject go = new GameObject("Wanderer - " + sprite.name);
            go.transform.SetParent(parent, false);
            go.transform.position = Iso(gx, gy);
            go.transform.localScale = sprite.name.Contains("bat") ? new Vector3(1.3f, 1.3f, 1f) : Vector3.one;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 120;
            go.AddComponent<NemesisDepthSort>().baseOrder = 120;

            Rigidbody2D body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.linearDamping = 3f;
            go.AddComponent<CircleCollider2D>().radius = 0.32f;

            NemesisEnemy enemy = go.AddComponent<NemesisEnemy>();
            enemy.health = sprite.name.Contains("gargoyle") ? 120 : sprite.name.Contains("revenant") ? 85 : 55;
            enemy.damage = sprite.name.Contains("gargoyle") ? 18 : 10;
            enemy.speed = sprite.name.Contains("bat") ? 2.15f : 1.35f;
            enemy.aggroRadius = 4.4f;
        }
    }

    private void BuildTileMesh(LoadedChunk chunk, int index, TileBatch batch)
    {
        Sprite sprite = terrain[index];
        if (sprite == null) return;

        Mesh mesh = new Mesh();
        mesh.name = "Infinite Ground Chunk Tile Mesh " + index;
        mesh.indexFormat = batch.vertices.Count > 60000
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(batch.vertices);
        mesh.SetUVs(0, batch.uvs);
        mesh.SetTriangles(batch.triangles, 0);
        mesh.RecalculateBounds();
        chunk.meshes.Add(mesh);

        GameObject layer = new GameObject("Ground Layer - " + sprite.name);
        layer.transform.SetParent(chunk.root.transform, false);
        layer.transform.localPosition = Vector3.zero;
        layer.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = layer.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = GetMaterial(sprite.texture);
        renderer.sortingOrder = index == 3 ? 2 : 0;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private Material GetMaterial(Texture texture)
    {
        int id = texture.GetInstanceID();
        if (materials.TryGetValue(id, out Material existing)) return existing;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        Material material = new Material(shader) { name = "Infinite Chunk - " + texture.name, mainTexture = texture };
        materials.Add(id, material);
        return material;
    }

    private void AddDiamond(TileBatch batch, Sprite sprite, Vector3 center)
    {
        Rect rect = sprite.textureRect;
        Texture texture = sprite.texture;
        float u0 = rect.xMin / texture.width;
        float u1 = rect.xMax / texture.width;
        float v0 = rect.yMin / texture.height;
        float v1 = rect.yMax / texture.height;
        int index = batch.vertices.Count;

        batch.vertices.Add(center + new Vector3(0f, cellHeight, 0f));
        batch.vertices.Add(center + new Vector3(cellWidth, 0f, 0f));
        batch.vertices.Add(center + new Vector3(0f, -cellHeight, 0f));
        batch.vertices.Add(center + new Vector3(-cellWidth, 0f, 0f));
        batch.uvs.Add(new Vector2((u0 + u1) * 0.5f, v1));
        batch.uvs.Add(new Vector2(u1, (v0 + v1) * 0.5f));
        batch.uvs.Add(new Vector2((u0 + u1) * 0.5f, v0));
        batch.uvs.Add(new Vector2(u0, (v0 + v1) * 0.5f));
        batch.triangles.Add(index); batch.triangles.Add(index + 1); batch.triangles.Add(index + 2);
        batch.triangles.Add(index); batch.triangles.Add(index + 2); batch.triangles.Add(index + 3);
    }

    private bool IsHandAuthoredCell(int x, int y)
    {
        return x >= -16 && x <= 16 && y >= -12 && y <= 12;
    }

    private Vector2Int WorldToCell(Vector3 world)
    {
        float a = world.x / cellWidth;
        float b = world.y / cellHeight;
        int x = Mathf.RoundToInt((a + b) * 0.5f);
        int y = Mathf.RoundToInt((b - a) * 0.5f);
        return new Vector2Int(x, y);
    }

    private Vector2Int CellToChunk(Vector2Int cell)
    {
        return new Vector2Int(Mathf.FloorToInt((float)cell.x / chunkSize), Mathf.FloorToInt((float)cell.y / chunkSize));
    }

    private Vector3 Iso(int x, int y)
    {
        return new Vector3((x - y) * cellWidth, (x + y) * cellHeight, 0f);
    }

    private static int CellHash(int x, int y)
    {
        unchecked { return (x * 73856093) ^ (y * 19349663) ^ 0x4A17B39D; }
    }

    private float Hash01(int x, int y, int salt)
    {
        unchecked
        {
            uint h = (uint)(CellHash(x + salt, y - salt) * 1664525 + worldSeed * 1013904223);
            h ^= h >> 16; h *= 2246822519u; h ^= h >> 13;
            return (h & 0x00FFFFFF) / 16777215f;
        }
    }
}


