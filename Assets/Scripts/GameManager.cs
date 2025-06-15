using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;  

[System.Serializable]
public struct EndpointPair
{
    public Vector2Int start;
    public Vector2Int end;
    public Material material;
};

public enum SolveMethod 
{
    DFS,
    BFS,
    AStar
};


public class GameManager : MonoBehaviour
{
    public GridManager gridManager;
    public float stepDelay = 0.05f;
    public int numPairs = 3;
    
    public SolveMethod method = SolveMethod.BFS;
    
    public Material pinkMaterial;

    private List<Material> palette;
    
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();


    // State
    private TileData[,] logicGrid;
    private List<EndpointPair> pairs;

    void Start()
    {
        palette = new List<Material>
        {
            Resources.Load<Material>("Materials/redMat"),
            Resources.Load<Material>("Materials/blueMat"),
            Resources.Load<Material>("Materials/greenMat"),
            Resources.Load<Material>("Materials/yellowMat")
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
        ClearLines();
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
        ClearLines();
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
        float offsetX = (W * gridManager.spacing) / 2f - gridManager.spacing / 2f;
        float offsetY = (H * gridManager.spacing) / 2f - gridManager.spacing / 2f;

        foreach (EndpointPair pair in pairs)
        {
            // Solve and collect visited nodes
            FlowSolver solver = new FlowSolver(logicGrid, new Vector2Int(W, H));
            List<Vector2Int> visitedNodes = new List<Vector2Int>();
            List<Vector2Int> path = method switch
            {
                SolveMethod.DFS   => solver.SolveDFS(pair.start, pair.end, visitedNodes),
                SolveMethod.BFS   => solver.SolveBFS(pair.start, pair.end, visitedNodes),
                SolveMethod.AStar => solver.SolveAStar(pair.start, pair.end, visitedNodes),
                _ => null
            };

            if (path == null)
            {
                Debug.LogWarning($"No path for {pair.material} using {method}. Aborting.");
                yield break;
            }

            // visualize the search in pink
            foreach (Vector2Int step in visitedNodes)
            {
                Tile tile = gridManager.GetTile(step);
                if (tile != null && !tile.IsEndpoint)
                {
                    tile.Highlight(pinkMaterial);
                    yield return new WaitForSeconds(stepDelay / 3f);
                }
            }

            // Draw the final solution path
            List<Vector3> linePoints = new List<Vector3>
            {
                new Vector3(
                    pair.start.x * gridManager.spacing - offsetX,
                    pair.start.y * gridManager.spacing - offsetY,
                    0f
                )
            };
            LineRenderer lineRenderer = new GameObject("PathLine")
                .AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = 0.7f;
            lineRenderer.endWidth = 0.7f;
            lineRenderer.numCornerVertices = 1;

            // Match color to endpoint material
            switch (pair.material.name)
            {
                case "redMat":    lineRenderer.material.color = Color.red;    break;
                case "blueMat":   lineRenderer.material.color = Color.blue;   break;
                case "greenMat":  lineRenderer.material.color = Color.green;  break;
                case "yellowMat": lineRenderer.material.color = Color.yellow; break;
            }

            foreach (Vector2Int step in path)
            {
                Tile tile = gridManager.GetTile(step);
                if (tile != null && !tile.IsEndpoint)
                {
                    tile.setMaterial(pair.material);
                    logicGrid[step.x, step.y].isBlocked = true;
                }

                linePoints.Add(new Vector3(
                    step.x * gridManager.spacing - offsetX,
                    step.y * gridManager.spacing - offsetY,
                    0f
                ));
                lineRenderer.positionCount = linePoints.Count;
                lineRenderer.SetPositions(linePoints.ToArray());
                lineRenderers.Add(lineRenderer);

                yield return new WaitForSeconds(stepDelay);
            }

            // Ensure endpoint is in the line
            linePoints.Add(new Vector3(
                pair.end.x * gridManager.spacing - offsetX,
                pair.end.y * gridManager.spacing - offsetY,
                0f
            ));
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPositions(linePoints.ToArray());
            lineRenderers.Add(lineRenderer);

            // Pause before cleanup
            yield return new WaitForSeconds(stepDelay * 3f);

            // reset all pink nodes
            foreach (Vector2Int step in visitedNodes)
            {
                Tile tile = gridManager.GetTile(step);
                if (tile != null && !tile.IsEndpoint && tile.GetMaterial() == pinkMaterial)
                {
                    tile.Clear();
                }
            }

            // optional small pause so reset is visible
            yield return new WaitForSeconds(stepDelay * 1.0f);
        }

        Debug.Log("All pairs solved!");
    }


    private void ClearLines()
    {
        if (lineRenderers != null)
        {
            foreach (LineRenderer line in lineRenderers)
            {
                Destroy(line.gameObject);
            }
            lineRenderers.Clear();
        }
    }
}
