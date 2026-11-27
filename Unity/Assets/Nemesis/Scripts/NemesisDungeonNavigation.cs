using System.Collections.Generic;
using UnityEngine;

public class NemesisDungeonNavigation : MonoBehaviour
{
    private bool[,] walkable;
    private float cellWidth;
    private float cellHeight;
    private static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    public void Configure(bool[,] grid, float width, float height)
    {
        walkable = grid;
        cellWidth = width;
        cellHeight = height;
    }

    public Vector2Int WorldToCell(Vector2 world)
    {
        float u = world.x / Mathf.Max(.001f, cellWidth);
        float v = world.y / Mathf.Max(.001f, cellHeight);
        return new Vector2Int(Mathf.RoundToInt((u + v) * .5f), Mathf.RoundToInt((v - u) * .5f));
    }

    public Vector2 CellToWorld(Vector2Int cell)
    {
        return new Vector2((cell.x - cell.y) * cellWidth, (cell.x + cell.y) * cellHeight);
    }

    public Vector2[] FindPath(Vector2 worldStart, Vector2 worldTarget)
    {
        if (walkable == null) return new Vector2[0];
        Vector2Int start = ClampToWalkable(WorldToCell(worldStart));
        Vector2Int goal = ClampToWalkable(WorldToCell(worldTarget));
        if (start == goal) return new Vector2[0];

        int width = walkable.GetLength(0), height = walkable.GetLength(1);
        int[,] costs = new int[width, height];
        bool[,] closed = new bool[width, height];
        Vector2Int[,] previous = new Vector2Int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++) costs[x, y] = int.MaxValue;

        List<Vector2Int> open = new List<Vector2Int>();
        costs[start.x, start.y] = 0;
        open.Add(start);
        bool found = false;
        int expanded = 0;
        while (open.Count > 0 && expanded < 1200)
        {
            int bestIndex = 0;
            int bestScore = int.MaxValue;
            for (int i = 0; i < open.Count; i++)
            {
                Vector2Int candidate = open[i];
                int score = costs[candidate.x, candidate.y] + Heuristic(candidate, goal);
                if (score < bestScore) { bestScore = score; bestIndex = i; }
            }

            Vector2Int current = open[bestIndex];
            open.RemoveAt(bestIndex);
            if (current == goal) { found = true; break; }
            if (closed[current.x, current.y]) continue;
            closed[current.x, current.y] = true;
            expanded++;

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next = current + direction;
                if (!Inside(next) || !walkable[next.x, next.y] || closed[next.x, next.y]) continue;
                int nextCost = costs[current.x, current.y] + 1;
                if (nextCost >= costs[next.x, next.y]) continue;
                costs[next.x, next.y] = nextCost;
                previous[next.x, next.y] = current;
                if (!open.Contains(next)) open.Add(next);
            }
        }
        if (!found) return new Vector2[0];

        List<Vector2> path = new List<Vector2>();
        Vector2Int step = goal;
        int guard = width * height;
        while (step != start && guard-- > 0)
        {
            path.Add(CellToWorld(step));
            step = previous[step.x, step.y];
        }
        path.Reverse();
        return path.ToArray();
    }

    Vector2Int ClampToWalkable(Vector2Int cell)
    {
        if (Inside(cell) && walkable[cell.x, cell.y]) return cell;
        int maxRadius = 6;
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
            {
                if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;
                Vector2Int candidate = cell + new Vector2Int(x, y);
                if (Inside(candidate) && walkable[candidate.x, candidate.y]) return candidate;
            }
        }
        return new Vector2Int(Mathf.Clamp(cell.x, 0, walkable.GetLength(0) - 1), Mathf.Clamp(cell.y, 0, walkable.GetLength(1) - 1));
    }

    bool Inside(Vector2Int cell) => walkable != null && cell.x >= 0 && cell.y >= 0 && cell.x < walkable.GetLength(0) && cell.y < walkable.GetLength(1);
    int Heuristic(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
