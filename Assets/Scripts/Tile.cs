using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    private Material defaultMat;
    
    private MeshRenderer meshRenderer;
    private Material? currentMaterial = null;

    public bool IsEndpoint { get; private set; }

    private void Awake()
    {
        defaultMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("Tile is missing a MeshRenderer!");
        }
    }

    public void Init(Vector2Int position)
    {
        GridPosition = position;
        Clear();
    }

    public void SetAsEndpoint(Material material)
    {
        IsEndpoint = true;
        setMaterial(material);
    }

    /*public void setMaterial(Color color)
    {
        currentMaterial = color;
        if (meshRenderer != null)
            meshRenderer.color = color;
    }*/
    public void setMaterial(Material material)
    {
        currentMaterial = material;
        meshRenderer.material = material;
    }

    public void Highlight(Material pathMaterial)
    {
        // Only highlight if not already an endpoint
        if (!IsEndpoint)
        {
            currentMaterial = pathMaterial;
            if (meshRenderer != null)
                meshRenderer.material = pathMaterial;
        }
    }

    public void Clear()
    {
        IsEndpoint = false;
        currentMaterial = null;
        if (meshRenderer != null)
            meshRenderer.material = defaultMat;
    }

    public bool IsOccupied()
    {
        return currentMaterial != null;
    }

    public Material? GetMaterial()
    {
        return currentMaterial;
    }
}