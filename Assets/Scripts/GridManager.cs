using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    // Grid settings
    public int width = 5;
    public int height = 5;
    public GameObject tilePrefab;
    public float spacing = 1f;
    private Tile[,] _grid;
    
    // Generate the grid of Spheres
    public void GenerateGrid()
    {
        _grid = new Tile[width, height];

        GenerateBackgroundGrid();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float gridOffsetX = (width * spacing) / 2f - spacing / 2f;
                float gridOffsetY = (height * spacing) / 2f - spacing / 2f;
                Vector2 position = new Vector2(x * spacing - gridOffsetX, y * spacing - gridOffsetY);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                
                Tile tile = tileObj.GetComponent<Tile>();
                tile.Init(new Vector2Int(x, y));
                
                
                // not real position, but simple position representation in grid
                // simpler for pathfinding
                _grid[x, y] = tile;
            }
        }
    }

    public Tile GetTile(Vector2Int position)
    {
        if (IsInBounds(position))
        {
            return _grid[position.x, position.y];
        }

        return null;
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
    }

    public void ClearGrid()
    {
        if (_grid == null) return;

        foreach (Tile tile in _grid)
        {
            tile.Clear();
        }
    }
    
    public Material gridLineMaterial;
    private List<GameObject> gridLines = new List<GameObject>();

    public void GenerateBackgroundGrid()
    {
        ClearBackgroundGrid();

        float gridOffsetX = (width * spacing) / 2f;
        float gridOffsetY = (height * spacing) / 2f;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(x * spacing - gridOffsetX, 0 - gridOffsetY, 0);
            Vector3 end   = new Vector3(x * spacing - gridOffsetX, height * spacing - gridOffsetY, 0);
            CreateLine(start, end);
        }

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(0 - gridOffsetX, y * spacing - gridOffsetY, 0);
            Vector3 end   = new Vector3(width * spacing - gridOffsetX, y * spacing - gridOffsetY, 0);
            CreateLine(start, end);
        }
    }


    private void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("GridLine");
        line.transform.parent = this.transform;

        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = gridLineMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.useWorldSpace = false;
        lr.SetPositions(new Vector3[] { start, end });
        lr.startColor = Color.gray;
        lr.endColor = Color.gray;

        gridLines.Add(line);
    }

    private void ClearBackgroundGrid()
    {
        foreach (GameObject line in gridLines)
        {
            if (line != null) Destroy(line);
        }
        gridLines.Clear();
    }

}