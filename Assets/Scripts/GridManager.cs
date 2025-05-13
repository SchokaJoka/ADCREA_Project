using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 5;
    public int height = 5;
    public GameObject tilePrefab;
    public float spacing = 1.1f;

    private Tile[,] grid;

    public Tile[,] Grid => grid;

    public void GenerateGrid()
    {
        grid = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x * spacing, y * spacing);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                Tile tile = tileObj.GetComponent<Tile>();

                tile.Init(new Vector2Int(x, y));
                grid[x, y] = tile;
            }
        }
    }

    public Tile GetTile(Vector2Int position)
    {
        if (IsInBounds(position))
        {
            return grid[position.x, position.y];
        }

        return null;
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
    }

    public void ClearGrid()
    {
        if (grid == null) return;

        foreach (var tile in grid)
        {
            tile.Clear();
        }
    }
}