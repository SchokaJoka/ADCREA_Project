using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Color? currentColor = null;

    public bool IsEndpoint { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Tile is missing a SpriteRenderer!");
        }
    }

    public void Init(Vector2Int position)
    {
        GridPosition = position;
        Clear();
    }

    public void SetAsEndpoint(Color color)
    {
        IsEndpoint = true;
        SetColor(color);
    }

    /*public void SetColor(Color color)
    {
        currentColor = color;
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }*/
    public void SetColor(Color color)
    {
        currentColor = color;
        spriteRenderer.color = color;
    }

    public void Highlight(Color pathColor)
    {
        // Only highlight if not already an endpoint
        if (!IsEndpoint)
        {
            currentColor = pathColor;
            if (spriteRenderer != null)
                spriteRenderer.color = pathColor;
        }
    }

    public void Clear()
    {
        IsEndpoint = false;
        currentColor = null;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    public bool IsOccupied()
    {
        return currentColor != null;
    }

    public Color? GetColor()
    {
        return currentColor;
    }
}