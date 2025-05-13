using System.Collections.Generic;
using UnityEngine;

public class FlowSolver
{
    private readonly Vector2Int gridSize;
    private readonly TileData[,] tiles;

    public FlowSolver(TileData[,] tiles, Vector2Int gridSize)
    {
        this.tiles = tiles;
        this.gridSize = gridSize;
    }

    // -------------------------------
    // 1) DFS + Backtracking Solver
    // -------------------------------
    public List<Vector2Int> SolveDFS(Vector2Int start, Vector2Int end)
    {
        var path = new List<Vector2Int>();
        var visited = new bool[gridSize.x, gridSize.y];
        if (DFS(start, end, visited, path, start, end))
            return path;
        return null;
    }

    private bool DFS(
        Vector2Int current,
        Vector2Int end,
        bool[,] visited,
        List<Vector2Int> path,
        Vector2Int start, 
        Vector2Int goal
    ) {
        if (!IsInBounds(current) || visited[current.x, current.y])
            return false;

        // Skip blocked tiles (unless it's start or end)
        if (tiles[current.x, current.y].isBlocked 
            && current != start && current != goal)
            return false;

        visited[current.x, current.y] = true;
        path.Add(current);

        if (current == end)
            return true;

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        foreach (var dir in dirs)
        {
            if (DFS(current + dir, end, visited, path, start, goal))
                return true;
        }

        // Backtrack
        path.RemoveAt(path.Count - 1);
        return false;
    }

    // -------------------------------
    // 2) BFS Shortest-Path Solver
    // -------------------------------
    public List<Vector2Int> SolveBFS(Vector2Int start, Vector2Int end)
    {
        var queue   = new Queue<Vector2Int>();
        var visited = new bool[gridSize.x, gridSize.y];
        var parent  = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            if (curr == end)
                break;

            foreach (var d in dirs)
            {
                var next = curr + d;
                if (!IsInBounds(next) || visited[next.x, next.y])
                    continue;

                // Skip blocked tiles (unless it's the end)
                if (tiles[next.x, next.y].isBlocked && next != end)
                    continue;

                visited[next.x, next.y] = true;
                parent[next] = curr;
                queue.Enqueue(next);
            }
        }

        if (!parent.ContainsKey(end))
            return null;

        var path = new List<Vector2Int>();
        var node = end;
        while (node != start)
        {
            path.Add(node);
            node = parent[node];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

    // -------------------------------
    // 3) Reachability Check via BFS
    // -------------------------------
    public bool IsReachableBFS(Vector2Int start, Vector2Int end)
    {
        var queue   = new Queue<Vector2Int>();
        var visited = new bool[gridSize.x, gridSize.y];

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            if (curr == end)
                return true;

            foreach (var d in dirs)
            {
                var next = curr + d;
                if (!IsInBounds(next) || visited[next.x, next.y])
                    continue;

                // Skip blocked tiles
                if (tiles[next.x, next.y].isBlocked)
                    continue;

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return false;
    }

    // -------------------------------
    // Helper: Bounds Check
    // -------------------------------
    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0
            && pos.x < gridSize.x && pos.y < gridSize.y;
    }
}
