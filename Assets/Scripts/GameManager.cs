using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EndpointPair
{
    public Vector2Int start;
    public Vector2Int end;
    public Material   material;
}

public enum SolveMethod { DFS, BFS }

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public float stepDelay = 0.1f;
    public int numPairs = 3;
    
    public SolveMethod method = SolveMethod.BFS;

    private List<Material> palette;

    // State
    private TileData[,] logicGrid;
    private List<EndpointPair> pairs;

    void Start()
    {
        palette = new List<Material>
        {
            Resources.Load<Material>("Materials/redMat"),
            Resources.Load<Material>("Materials/blueMat"),
            Resources.Load<Material>("Materials/greenMat")
        };
        
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
        PlaceEndpoints();
    }

    public void OnMethodChanged(int idx)
    {
        method = (SolveMethod)idx;
    }

    private void InitializeLogicGrid()
    {
        int W = gridManager.width, H = gridManager.height;
        logicGrid = new TileData[W, H];
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                logicGrid[x, y] = new TileData(new Vector2Int(x, y));
    }

    private void GeneratePairs()
    {
        pairs = new List<EndpointPair>();
        HashSet<Vector2Int> used = new HashSet<Vector2Int>();
        int W = gridManager.width, H = gridManager.height;

        for (int i = 0; i < numPairs; i++)
        {
            Vector2Int s, e;
            do { s = new Vector2Int(Random.Range(0, W), Random.Range(0, H)); }
            while (!used.Add(s));
            do { e = new Vector2Int(Random.Range(0, W), Random.Range(0, H)); }
            while (!used.Add(e));

            pairs.Add(new EndpointPair {
                start = s,
                end   = e,
                material = palette[i]
            });
        }
    }

    private void PlaceEndpoints()
    {
        foreach (EndpointPair pair in pairs)
        {
            Tile tileStart = gridManager.GetTile(pair.start);
            Tile tileEnd = gridManager.GetTile(pair.end);
            tileStart.SetAsEndpoint(pair.material);
            tileEnd.SetAsEndpoint(pair.material);
            logicGrid[pair.start.x, pair.start.y].isBlocked = true;
            logicGrid[pair.end.x,   pair.end.y  ].isBlocked = true;
        }
    }

    private IEnumerator SolveAllPairsSequentially()
    {
        int W = gridManager.width, H = gridManager.height;

        foreach (EndpointPair pair in pairs)
        {
            // pick the solver method
            FlowSolver solver = new FlowSolver(logicGrid, new Vector2Int(W, H));
            List<Vector2Int> path = (method == SolveMethod.DFS)
                ? solver.SolveDFS(pair.start, pair.end)
                : solver.SolveBFS(pair.start, pair.end);
            
            // Check if solver found a path
            if (path == null)
            {
                Debug.LogWarning($"No path for {pair.material} using {method}. Aborting.");
                yield break;
            }

            Debug.Log($"{method} path for {pair.material}: {path.Count} steps");

            // Path ANIMATION
            foreach (Vector2Int step in path)
            {
                Tile tile = gridManager.GetTile(step);
                if (tile != null && !tile.IsEndpoint)
                {
                    tile.setMaterial(pair.material);
                    logicGrid[step.x, step.y].isBlocked = true;
                }
                yield return new WaitForSeconds(stepDelay);
            }
            yield return new WaitForSeconds(stepDelay * 3);
        }
        Debug.Log("All pairs solved!");
    }
}
