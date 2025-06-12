using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 5;
    public int height = 5;
    public GameObject tilePrefab;
    public float spacing = 1f;

    private Tile[,] _grid;

    // Comment: We don't need this???
    // public Tile[,] grid => _grid;

    public void GenerateGrid()
    {
        _grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x * spacing, y * spacing);
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
}