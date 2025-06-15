using System.Collections.Generic;
using UnityEngine;

public class FlowSolver
{
    private readonly Vector2Int gridSize;
    private readonly TileData[,] tiles;

    // Constructor
    public FlowSolver(TileData[,] tiles, Vector2Int gridSize)
    {
        this.tiles = tiles;
        this.gridSize = gridSize;
    }
    
    //DFS + Backtracking Solver
    public List<Vector2Int> SolveDFS(Vector2Int start, Vector2Int end, List<Vector2Int> visitedNodes = null)
    {
        bool[,] visited = new bool[gridSize.x, gridSize.y];
        List<Vector2Int> path = new List<Vector2Int>();

        // Kick off the recursive DFS
        bool found = DFS(
            start,
            end,
            visited,
            path,
            start,
            end,
            visitedNodes
        );

        return found ? path : null;
    }

    private bool DFS(
        Vector2Int current,
        Vector2Int end,
        bool[,] visited,
        List<Vector2Int> path,
        Vector2Int start,
        Vector2Int goal,
        List<Vector2Int> visitedNodes  // ← new parameter
    )
    {
        if (!IsInBounds(current) || visited[current.x, current.y])
            return false;

        // Skip blocked tiles (unless it's start or end)
        if (tiles[current.x, current.y].isBlocked
            && current != start && current != goal)
            return false;

        // Mark visited & record for visualization
        visited[current.x, current.y] = true;
        visitedNodes?.Add(current);

        path.Add(current);

        // Found the end?
        if (current == end)
            return true;

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        foreach (Vector2Int dir in dirs)
        {
            if (DFS(current + dir, end, visited, path, start, goal, visitedNodes))
                return true;
        }

        // Backtrack
        path.RemoveAt(path.Count - 1);
        return false;
    }

    // -------------------------------
    // 2) BFS Shortest-Path Solver
    // -------------------------------
    public List<Vector2Int> SolveBFS(
        Vector2Int start,
        Vector2Int end,
        List<Vector2Int> visitedNodes = null
    )
    {
        var queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[gridSize.x, gridSize.y];
        var parent = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;
        visitedNodes?.Add(start);

        Vector2Int[] dirs = {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        };

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();

            // Optionally record the dequeue as "thinking"
            visitedNodes?.Add(curr);

            if (curr == end)
                break;

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = curr + dir;
                if (!IsInBounds(next) || visited[next.x, next.y])
                    continue;

                if (tiles[next.x, next.y].isBlocked && next != end)
                    continue;

                visited[next.x, next.y] = true;
                visitedNodes?.Add(next);

                parent[next] = curr;
                queue.Enqueue(next);
            }
        }

        if (!parent.ContainsKey(end))
            return null;

        var path = new List<Vector2Int>();
        Vector2Int node = end;
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
        var queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[gridSize.x, gridSize.y];

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
            Vector2Int curr = queue.Dequeue();
            if (curr == end)
                return true;

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = curr + dir;
                if (!IsInBounds(next) || visited[next.x, next.y])
                    continue;

                if (tiles[next.x, next.y].isBlocked)
                    continue;

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return false;
    }

    // -------------------------------
    // 4) A* Shortest-Path Solver
    // -------------------------------
    public List<Vector2Int> SolveAStar(
        Vector2Int start,
        Vector2Int end,
        List<Vector2Int> visitedNodes = null
    )
    {
        var openSet = new PriorityQueue<Vector2Int>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        openSet.Enqueue(start, 0);
        gScore[start] = 0f;
        fScore[start] = Heuristic(start, end);
        visitedNodes?.Add(start);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet.Dequeue();
            visitedNodes?.Add(current);

            if (current == end)
                return ReconstructPath(cameFrom, current);

            closedSet.Add(current);

            Vector2Int[] dirs = {
                Vector2Int.right, Vector2Int.up,
                Vector2Int.left,  Vector2Int.down
            };

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int neighbor = current + dir;

                if (!IsInBounds(neighbor) || closedSet.Contains(neighbor))
                    continue;

                if (tiles[neighbor.x, neighbor.y].isBlocked && neighbor != end)
                    continue;

                float tentativeG = gScore[current] + 1f;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, end);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                        visitedNodes?.Add(neighbor);
                    }
                }
            }
        }

        return null; // No path found
    }

    // Heuristic for A*
    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // Reconstruct path for A*
    private List<Vector2Int> ReconstructPath(
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        Vector2Int current
    )
    {
        List<Vector2Int> totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    // Helper: check bounds
    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0
            && pos.x < gridSize.x && pos.y < gridSize.y;
    }
}
