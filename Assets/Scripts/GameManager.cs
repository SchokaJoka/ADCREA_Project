using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GridManager gridManager;

    private void Start()
    {
        //Generate the visual grid
        gridManager.GenerateGrid();

        //start/end points
        Vector2Int start = new Vector2Int(0, 0);
        Vector2Int end   = new Vector2Int(4, 4);

        //Mark them visually as endpoints
        gridManager.GetTile(start).SetAsEndpoint(Color.red);
        gridManager.GetTile(end)  .SetAsEndpoint(Color.red);

        //Build a matching logical grid for the solver
        TileData[,] logicGrid = new TileData[gridManager.width, gridManager.height];
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                logicGrid[x, y] = new TileData(new Vector2Int(x, y));
            }
        }

        //Instantiate the solver
        var solver = new FlowSolver(logicGrid, new Vector2Int(gridManager.width, gridManager.height));

        //Solve (DFS or BFS—here we use DFS)
        List<Vector2Int> path = solver.SolveDFS(start, end);

        //highlight the solution path (temp, is hardcoded rn)
        if (path != null)
        {
            Debug.Log("Path found:");
            foreach (var step in path)
            {
                Debug.Log(step);
            }

            // Now color the tiles
            foreach (var step in path)
            {
                Tile tile = gridManager.GetTile(step);
                if (tile != null && !tile.IsEndpoint)
                    tile.SetColor(Color.yellow);
            }
        }
        else
        {
            Debug.Log("No path found.");
        }
    }
}