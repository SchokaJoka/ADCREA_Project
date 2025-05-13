using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EndpointPair
{
    public Vector2Int start;
    public Vector2Int end;
    public Color     color;
}

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    [Header("Animation Settings")]
    [Tooltip("Delay (in seconds) between coloring each tile")]
    public float stepDelay = 0.1f;

    [Header("Random Pairs Settings")]
    public int numPairs = 3;
    private readonly List<Color> palette = new List<Color>
        { Color.red, Color.blue, Color.green, Color.magenta, Color.cyan, Color.yellow };

    private void Start()
    {
        // 1) Build the visual grid
        gridManager.GenerateGrid();
        int W = gridManager.width, H = gridManager.height;

        // 2) Build the logic grid
        TileData[,] logicGrid = new TileData[W, H];
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                logicGrid[x, y] = new TileData(new Vector2Int(x, y));

        // 3) Generate random endpoint pairs (no two endpoints overlap)
        var used    = new HashSet<Vector2Int>();
        var pairs   = new List<EndpointPair>();
        for (int i = 0; i < numPairs; i++)
        {
            Vector2Int s, e;
            do { s = new Vector2Int(Random.Range(0, W), Random.Range(0, H)); }
            while (!used.Add(s));
            do { e = new Vector2Int(Random.Range(0, W), Random.Range(0, H)); }
            while (!used.Add(e));
            pairs.Add(new EndpointPair { start = s, end = e, color = palette[i] });
        }

        // 4) **Place all endpoints up front** and reserve them in logicGrid
        foreach (var p in pairs)
        {
            var tS = gridManager.GetTile(p.start);
            var tE = gridManager.GetTile(p.end);
            tS.SetAsEndpoint(p.color);
            tE.SetAsEndpoint(p.color);

            logicGrid[p.start.x, p.start.y].isBlocked = true;
            logicGrid[p.end.x,   p.end.y  ].isBlocked = true;
        }

        // 5) Now solve/animate each pair in turn
        StartCoroutine(SolveAllPairsSequentially(pairs, logicGrid, W, H));
    }

    private IEnumerator SolveAllPairsSequentially(
        List<EndpointPair> pairs,
        TileData[,] logicGrid,
        int W, int H
    ){
        foreach (var pair in pairs)
        {
            // a) Solve shortest path around all reserved blocks
            var solver = new FlowSolver(logicGrid, new Vector2Int(W, H));
            var path   = solver.SolveBFS(pair.start, pair.end);
            if (path == null)
            {
                Debug.LogWarning($"No path found for {pair.color}. Stopping.");
                yield break;
            }

            Debug.Log($"Animating path for {pair.color}: {path.Count} steps");

            // b) Animate coloring + reserve those tiles
            foreach (var step in path)
            {
                var tile = gridManager.GetTile(step);
                if (tile != null && !tile.IsEndpoint)
                {
                    tile.SetColor(pair.color);
                    logicGrid[step.x, step.y].isBlocked = true;
                }
                yield return new WaitForSeconds(stepDelay);
            }

            // c) Brief pause before next
            yield return new WaitForSeconds(stepDelay * 3);
        }

        Debug.Log("All pairs solved!");
    }
}
