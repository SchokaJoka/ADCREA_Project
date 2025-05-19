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
    public GridManager gridManager;
    public float stepDelay = 0.1f;
    public int numPairs = 3;

    //palette
    private readonly List<Color> palette = new List<Color>
    {
        Color.red, Color.blue, Color.green, Color.magenta, Color.cyan, Color.yellow
    };

    // State
    private TileData[,]          logicGrid;
    private List<EndpointPair>   pairs;

    void Start()
    {
        // Initial setup
        gridManager.GenerateGrid();
        InitializeLogicGrid();
        GeneratePairs();
        PlaceEndpoints();
    }
    
    public void ShufflePairs()
    {
        StopAllCoroutines();
        gridManager.ClearGrid();
        InitializeLogicGrid();
        GeneratePairs();
        PlaceEndpoints();
    }
    
    public void Play()
    {
        StopAllCoroutines();
        StartCoroutine(SolveAllPairsSequentially());
    }
    
    public void ResetGrid()
    {
        StopAllCoroutines();
        gridManager.ClearGrid();
        InitializeLogicGrid();
        PlaceEndpoints(); // re-draw the endpoints without re-shuffling
    }
    
    private void InitializeLogicGrid()
    {
        int W = gridManager.width, H = gridManager.height;
        logicGrid = new TileData[W, H];
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                logicGrid[x, y] = new TileData(new Vector2Int(x, y));
    }

    // Randomly pick N non-overlapping pairs
    private void GeneratePairs()
    {
        pairs = new List<EndpointPair>();
        var used  = new HashSet<Vector2Int>();
        int W = gridManager.width, H = gridManager.height;

        for (int i = 0; i < numPairs; i++)
        {
            // pick start
            Vector2Int s;
            do { s = new Vector2Int(Random.Range(0, W), Random.Range(0, H)); }
            while (!used.Add(s));

            // pick end
            Vector2Int e;
            do { e = new Vector2Int(Random.Range(0, W), Random.Range(0, H)); }
            while (!used.Add(e));

            pairs.Add(new EndpointPair {
                start = s,
                end   = e,
                color = palette[i]
            });
        }
    }

    // Draw all endpoints and reserve them in logicGrid
    private void PlaceEndpoints()
    {
        foreach (var p in pairs)
        {
            var tS = gridManager.GetTile(p.start);
            var tE = gridManager.GetTile(p.end);
            tS.SetAsEndpoint(p.color);
            tE.SetAsEndpoint(p.color);

            logicGrid[p.start.x, p.start.y].isBlocked = true;
            logicGrid[p.end.x,   p.end.y  ].isBlocked = true;
        }
    }

    // Solve & animate each pair in turn
    private IEnumerator SolveAllPairsSequentially()
    {
        int W = gridManager.width, H = gridManager.height;

        foreach (var pair in pairs)
        {
            var solver = new FlowSolver(logicGrid, new Vector2Int(W, H));
            var path   = solver.SolveBFS(pair.start, pair.end);
            if (path == null)
            {
                Debug.LogWarning($"No path for {pair.color}. Stopping.");
                yield break;
            }

            Debug.Log($"Animating {pair.color}: {path.Count} steps");
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

            // small pause before next color
            yield return new WaitForSeconds(stepDelay * 3);
        }

        Debug.Log("All pairs solved!");
    }
}
