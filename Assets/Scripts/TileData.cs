using UnityEngine;

public class TileData
{
    public Vector2Int position;
    public bool isBlocked;  // Optional: handle future blocked tiles

    public TileData(Vector2Int pos)
    {
        position = pos;
        isBlocked = false;
    }
}
