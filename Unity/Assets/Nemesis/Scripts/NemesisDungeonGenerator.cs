using System;
using System.Collections.Generic;
using UnityEngine;

public class NemesisDungeonGenerator : MonoBehaviour
{
    public enum RoomType { Entrance, Combat, Treasure, Shrine, Boss }

    [Header("Generation")]
    [Tooltip("0 = random layout on every generation. Any other value reproduces a fixed layout.")] public int seed = 0;
    [Range(5, 16)] public int roomCount = 10;
    public Vector2Int mapSize = new Vector2Int(64, 64);
    [Range(1, 5)] public int corridorWidth = 3;
    [Header("Fog of war")][Range(3, 16)] public int visionRadius = 8;
    [Header("Floor and architecture")]
    public Sprite[] floorVariants;
    public Sprite wallSprite, archSprite, staircaseSprite;
    [Header("Room props")]
    public Sprite torchSprite, barrelSprite, webSprite, mossSprite, crystalSprite, trapSprite;
    public Sprite chestSprite, pillarSprite, cageSprite, mushroomSprite, altarSprite;
    [Header("Characters")]
    public Sprite[] enemyVariants;
    public Sprite bossSprite, playerSprite;
    [Header("World scale")]
    public float cellWidth = 0.72f;
    public float cellHeight = 0.36f;
    private System.Random rng;
    private bool[,] floor;
    private bool[,] reserved;
    private readonly List<RectInt> rooms = new List<RectInt>();
    private readonly List<RoomType> roomTypes = new List<RoomType>();
    private readonly List<Vector2Int> corridorMidpoints = new List<Vector2Int>();
    private Transform generated;
    private int activeSeed;

    void Start() { Generate(); }
    void Update() { if (Input.GetKeyDown(KeyCode.F5)) Generate(); }

    public void Generate()
    {
        if (generated != null) Destroy(generated.gameObject);
        if (mapSize.x < 32 || mapSize.y < 32) mapSize = new Vector2Int(64, 64);
        activeSeed = seed == 0 ? (Guid.NewGuid().GetHashCode() & int.MaxValue) : seed;
        if (activeSeed == 0) activeSeed = 1;
        rng = new System.Random(activeSeed);
        floor = new bool[mapSize.x, mapSize.y];
        reserved = new bool[mapSize.x, mapSize.y];
        rooms.Clear(); roomTypes.Clear(); corridorMidpoints.Clear();
        generated = new GameObject("Generated Dungeon - Seed " + activeSeed).transform;

        BuildRooms();
        if (rooms.Count == 0) return;
        AssignRoomTypes();
        BuildCorridors();
        ValidateConnectivity();
        var navigation = GetComponent<NemesisDungeonNavigation>();
        if (navigation == null) navigation = gameObject.AddComponent<NemesisDungeonNavigation>();
        navigation.Configure(floor, cellWidth, cellHeight);
        BuildFloorAndWalls();
        DecorateRooms();
        SpawnActors();
        PlacePlayerAndCamera();
        var fog = GetComponent<NemesisFogOfWar>();
        if (fog == null) fog = gameObject.AddComponent<NemesisFogOfWar>();
        fog.Configure(floor, cellWidth, cellHeight, visionRadius, Vector2Int.zero);
        Physics2D.SyncTransforms();
        NemesisHUD.Instance?.SetMessage("ЗАБЫТЫЕ КАТАКОМБЫ  |  ЗЕРНО " + activeSeed + "  |  F5: НОВАЯ ГЕНЕРАЦИЯ");
        Vector2[] routeCheck = rooms.Count > 1 ? navigation.FindPath(Iso(RoomCenter(rooms[0])), Iso(RoomCenter(rooms[rooms.Count - 1]))) : new Vector2[0];
        int enemies = generated.GetComponentsInChildren<NemesisEnemy>().Length;
        int chests = generated.GetComponentsInChildren<NemesisChest>().Length;
        int traps = generated.GetComponentsInChildren<NemesisDamageTrap>().Length;
        int shrines = generated.GetComponentsInChildren<NemesisShrine>().Length;
        Debug.Log("NEMESIS DUNGEON: seed=" + activeSeed + ", rooms=" + rooms.Count + ", routeCells=" + routeCheck.Length + ", enemies=" + enemies + ", chests=" + chests + ", traps=" + traps + ", shrines=" + shrines + ", types=" + string.Join(",", roomTypes));
    }

    void BuildRooms()
    {
        int attempts = Mathf.Max(rooms.Count + 1, roomCount * 120);
        int maxW = Mathf.Min(13, mapSize.x - 8), maxH = Mathf.Min(12, mapSize.y - 8);
        for (int i = 0; i < attempts && rooms.Count < roomCount; i++)
        {
            int w = rng.Next(7, maxW + 1), h = rng.Next(7, maxH + 1);
            int x = rng.Next(2, mapSize.x - w - 2), y = rng.Next(2, mapSize.y - h - 2);
            RectInt candidate = new RectInt(x, y, w, h);
            bool overlaps = false;
            foreach (var existing in rooms)
                if (Expand(existing, 2).Overlaps(candidate)) { overlaps = true; break; }
            if (overlaps) continue;
            rooms.Add(candidate);
            for (int xx = x; xx < x + w; xx++)
                for (int yy = y; yy < y + h; yy++) floor[xx, yy] = true;
        }
        if (rooms.Count < 5)
        {
            Debug.LogWarning("NEMESIS DUNGEON: only " + rooms.Count + " rooms fitted. Increase mapSize for a larger layout.");
        }
    }

    void AssignRoomTypes()
    {
        for (int i = 0; i < rooms.Count; i++) roomTypes.Add(RoomType.Combat);
        roomTypes[0] = RoomType.Entrance;
        if (rooms.Count > 1) roomTypes[rooms.Count - 1] = RoomType.Boss;
        if (rooms.Count > 3)
        {
            int treasure = Mathf.Clamp(rooms.Count / 2, 1, rooms.Count - 2);
            roomTypes[treasure] = RoomType.Treasure;
            int shrine = treasure == 1 ? 2 : 1;
            if (shrine < rooms.Count - 1) roomTypes[shrine] = RoomType.Shrine;
        }
    }

    RectInt Expand(RectInt r, int n) => new RectInt(r.xMin - n, r.yMin - n, r.width + n * 2, r.height + n * 2);
    Vector2Int RoomCenter(RectInt r) => new Vector2Int(r.xMin + r.width / 2, r.yMin + r.height / 2);
    Vector3 Iso(Vector2Int c) => new Vector3((c.x - c.y) * cellWidth, (c.x + c.y) * cellHeight, 0);

    void BuildCorridors()
    {
        for (int i = 1; i < rooms.Count; i++)
            ConnectRooms(RoomCenter(rooms[i - 1]), RoomCenter(rooms[i]));
        // A few extra links create loops, so exploration is not always a single hallway chain.
        for (int i = 2; i < rooms.Count; i++)
            if (rng.NextDouble() < 0.24) ConnectRooms(RoomCenter(rooms[i]), RoomCenter(rooms[rng.Next(0, i - 1)]));
    }

    void ConnectRooms(Vector2Int a, Vector2Int b)
    {
        Vector2Int mid;
        if (rng.Next(2) == 0)
        {
            CarveX(a.x, b.x, a.y); CarveY(a.y, b.y, b.x);
            mid = new Vector2Int((a.x + b.x) / 2, a.y);
        }
        else
        {
            CarveY(a.y, b.y, a.x); CarveX(a.x, b.x, b.y);
            mid = new Vector2Int(a.x, (a.y + b.y) / 2);
        }
        corridorMidpoints.Add(mid);
    }

    void CarveX(int a, int b, int y)
    {
        int half = corridorWidth / 2;
        for (int x = Mathf.Min(a, b); x <= Mathf.Max(a, b); x++)
            for (int yy = y - half; yy <= y + half; yy++) SetFloor(x, yy);
    }
    void CarveY(int a, int b, int x)
    {
        int half = corridorWidth / 2;
        for (int y = Mathf.Min(a, b); y <= Mathf.Max(a, b); y++)
            for (int xx = x - half; xx <= x + half; xx++) SetFloor(xx, y);
    }
    void SetFloor(int x, int y)
    {
        if (x >= 1 && y >= 1 && x < mapSize.x - 1 && y < mapSize.y - 1) floor[x, y] = true;
    }

    void ValidateConnectivity()
    {
        if (rooms.Count < 2) return;
        bool[,] visited = FloodFrom(RoomCenter(rooms[0]));
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int c = RoomCenter(rooms[i]);
            if (!visited[c.x, c.y])
            {
                Debug.LogWarning("NEMESIS DUNGEON: repairing disconnected room " + i);
                ConnectRooms(RoomCenter(rooms[i - 1]), c);
                visited = FloodFrom(RoomCenter(rooms[0]));
            }
        }
    }

    bool[,] FloodFrom(Vector2Int start)
    {
        bool[,] visited = new bool[mapSize.x, mapSize.y];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start); visited[start.x, start.y] = true;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            foreach (var d in dirs)
            {
                Vector2Int n = p + d;
                if (n.x < 0 || n.y < 0 || n.x >= mapSize.x || n.y >= mapSize.y) continue;
                if (!floor[n.x, n.y] || visited[n.x, n.y]) continue;
                visited[n.x, n.y] = true; queue.Enqueue(n);
            }
        }
        return visited;
    }

    void BuildFloorAndWalls()
    {
        for (int x = 0; x < mapSize.x; x++)
        for (int y = 0; y < mapSize.y; y++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            if (floor[x, y])
            {
                Sprite s = Pick(floorVariants);
                if (s != null) MakeSprite("Floor", cell, s, 0, new Vector2(1.03f, 0.72f));
            }
            else if (HasCardinalFloorNeighbour(x, y) && wallSprite != null)
            {
                GameObject wall = MakeSprite("Stone Wall", cell, wallSprite, 10, new Vector2(1.05f, 1.05f));
                BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.58f, 0.48f);
            }
        }
    }

    bool HasCardinalFloorNeighbour(int x, int y)
    {
        return IsFloor(x + 1, y) || IsFloor(x - 1, y) || IsFloor(x, y + 1) || IsFloor(x, y - 1);
    }
    bool IsFloor(int x, int y) => x >= 0 && y >= 0 && x < mapSize.x && y < mapSize.y && floor[x, y];

    bool TryReserve(RectInt r, int margin, Vector2Int avoid, int avoidRadius, out Vector2Int cell)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            int x = rng.Next(r.xMin + margin, r.xMax - margin);
            int y = rng.Next(r.yMin + margin, r.yMax - margin);
            cell = new Vector2Int(x, y);
            if (!floor[x, y] || reserved[x, y]) continue;
            if (Mathf.Abs(x - avoid.x) <= avoidRadius && Mathf.Abs(y - avoid.y) <= avoidRadius) continue;
            reserved[x, y] = true; return true;
        }
        cell = Vector2Int.zero; return false;
    }

    void DecorateRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            RectInt r = rooms[i]; Vector2Int center = RoomCenter(r);
            // Random edge details: never place blocking props on the center route.
            int props = rng.Next(2, 5);
            for (int n = 0; n < props; n++)
            {
                if (!TryReserve(r, 2, center, 2, out Vector2Int p)) continue;
                int choice = rng.Next(5);
                Sprite sprite = choice == 0 ? pillarSprite : choice == 1 ? barrelSprite : choice == 2 ? webSprite : choice == 3 ? cageSprite : mushroomSprite;
                if (sprite != null) MakeSprite("Room Detail", p, sprite, 4, choice == 0 ? new Vector2(.8f, 1.15f) : Vector2.one);
            }
            if (torchSprite != null)
            {
                MakeSprite("Wall Torch", new Vector2Int(r.xMin + 1, r.yMin + 1), torchSprite, 8, Vector2.one);
                MakeSprite("Wall Torch", new Vector2Int(r.xMax - 2, r.yMax - 2), torchSprite, 8, Vector2.one);
            }
            switch (roomTypes[i])
            {
                case RoomType.Entrance: BuildEntranceRoom(r, center); break;
                case RoomType.Treasure: BuildTreasureRoom(r, center); break;
                case RoomType.Shrine: BuildShrineRoom(r, center); break;
                case RoomType.Boss: BuildBossRoom(r, center); break;
                default: BuildCombatRoom(r, center); break;
            }
        }
        // Decorative arches punctuate some longer connector paths.
        if (archSprite != null)
            for (int i = 0; i < corridorMidpoints.Count; i += 2)
                MakeSprite("Ancient Arch", corridorMidpoints[i], archSprite, 6, new Vector2(1.05f, 1.05f));
    }

    void BuildEntranceRoom(RectInt r, Vector2Int center)
    {
        reserved[center.x, center.y] = true;
        Vector2Int portalCell = new Vector2Int(r.xMin + 2, r.yMax - 2);
        if (staircaseSprite != null)
        {
            GameObject exit = MakeSprite("Return to Ashen Outskirts", portalCell, staircaseSprite, 8, Vector2.one);
            exit.AddComponent<CircleCollider2D>().isTrigger = true;
            exit.AddComponent<NemesisDungeonPortal>().destinationScene = "AshenOutskirts";
            reserved[portalCell.x, portalCell.y] = true;
        }
        if (mossSprite != null && TryReserve(r, 2, center, 2, out Vector2Int moss)) MakeSprite("Moss", moss, mossSprite, 2, Vector2.one);
    }

    void BuildCombatRoom(RectInt r, Vector2Int center)
    {
        if (rng.NextDouble() < 0.65 && TryReserve(r, 2, center, 2, out Vector2Int trap))
        {
            GameObject hazard = MakeSprite("Pressure Spike Trap", trap, trapSprite, 3, Vector2.one);
            if (hazard != null)
            {
                CircleCollider2D trigger = hazard.AddComponent<CircleCollider2D>(); trigger.isTrigger = true; trigger.radius = 0.38f;
                hazard.AddComponent<NemesisDamageTrap>(); reserved[trap.x, trap.y] = true;
            }
        }
    }

    void BuildTreasureRoom(RectInt r, Vector2Int center)
    {
        int count = rng.Next(2, 4);
        for (int i = 0; i < count; i++)
        {
            if (!TryReserve(r, 2, center, 1, out Vector2Int p)) continue;
            if (chestSprite == null) continue;
            GameObject chest = MakeSprite("Relic Chest", p, chestSprite, 7, Vector2.one);
            chest.AddComponent<BoxCollider2D>().isTrigger = true; chest.AddComponent<NemesisChest>();
        }
        if (crystalSprite != null && TryReserve(r, 2, center, 1, out Vector2Int crystal)) MakeSprite("Veil Crystal", crystal, crystalSprite, 5, Vector2.one);
    }

    void BuildShrineRoom(RectInt r, Vector2Int center)
    {
        Vector2Int p = center + new Vector2Int(1, 0);
        if (!reserved[p.x, p.y])
        {
            Sprite shrine = altarSprite != null ? altarSprite : crystalSprite;
            if (shrine != null)
            {
                GameObject altar = MakeSprite("Ashen Shrine", p, shrine, 7, new Vector2(1.1f, 1.1f));
                altar.AddComponent<CircleCollider2D>().isTrigger = true; altar.AddComponent<NemesisShrine>(); reserved[p.x, p.y] = true;
            }
        }
        for (int i = 0; i < 2; i++)
            if (TryReserve(r, 2, center, 1, out Vector2Int detail) && crystalSprite != null) MakeSprite("Shrine Crystal", detail, crystalSprite, 5, Vector2.one);
    }

    void BuildBossRoom(RectInt r, Vector2Int center)
    {
        if (staircaseSprite != null)
        {
            GameObject exit = MakeSprite("Guardian Gate", center + new Vector2Int(0, -2), staircaseSprite, 5, Vector2.one);
            exit.AddComponent<CircleCollider2D>().isTrigger = true;
            var portal = exit.AddComponent<NemesisDungeonPortal>(); portal.destinationScene = "AshenOutskirts"; portal.requiresBossDefeated = true;
        }
        if (cageSprite != null && TryReserve(r, 2, center, 2, out Vector2Int cage)) MakeSprite("Broken Iron Cage", cage, cageSprite, 4, Vector2.one);
    }

    void SpawnActors()
    {
        for (int i = 1; i < rooms.Count - 1; i++)
        {
            RectInt r = rooms[i]; int count = roomTypes[i] == RoomType.Treasure || roomTypes[i] == RoomType.Shrine ? 1 : rng.Next(2, 4);
            for (int n = 0; n < count; n++)
            {
                if (!TryReserve(r, 2, RoomCenter(r), 1, out Vector2Int cell)) continue;
                Sprite sprite = Pick(enemyVariants);
                if (sprite == null) continue;
                CreateEnemy("Dungeon Wraith", cell, sprite, 55, 9, 1.35f, 3.8f);
            }
        }
        if (rooms.Count > 1 && bossSprite != null)
            CreateEnemy("The Hollow Knight", RoomCenter(rooms[rooms.Count - 1]), bossSprite, 520, 24, 1.05f, 6f, new Vector2(1.8f, 1.8f));
    }

    void CreateEnemy(string name, Vector2Int cell, Sprite sprite, int hp, int damage, float speed, float aggro, Vector2? scale = null)
    {
        GameObject go = MakeSprite(name, cell, sprite, 12, scale ?? new Vector2(.95f, 1.05f));
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.freezeRotation = true; rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        CircleCollider2D col = go.AddComponent<CircleCollider2D>(); col.radius = .32f;
        NemesisEnemy enemy = go.AddComponent<NemesisEnemy>(); enemy.health = hp; enemy.damage = damage; enemy.speed = speed; enemy.aggroRadius = aggro;
    }

    void PlacePlayerAndCamera()
    {
        Vector2Int start = RoomCenter(rooms[0]);
        NemesisPlayer p = FindFirstObjectByType<NemesisPlayer>();
        if (p != null) p.transform.position = Iso(start);
        Camera cam = Camera.main;
        if (cam != null) cam.transform.position = new Vector3(Iso(start).x, Iso(start).y, -10);
    }

    GameObject MakeSprite(string name, Vector2Int cell, Sprite sprite, int order, Vector2 scale)
    {
        if (sprite == null) return null;
        GameObject go = new GameObject(name); go.transform.SetParent(generated, false); go.transform.position = Iso(cell); go.transform.localScale = scale;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite;
        sr.sortingOrder = order - Mathf.RoundToInt(go.transform.position.y * 10f);
        return go;
    }

    Sprite Pick(Sprite[] choices) => choices == null || choices.Length == 0 ? null : choices[rng.Next(choices.Length)];
}






