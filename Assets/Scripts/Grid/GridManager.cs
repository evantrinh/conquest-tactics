using UnityEngine;

public class GridManager : MonoBehaviour, IGrid
{
    public LevelData level;
    public TileSet tileSet;
    public float heightStep = 0.5f;

    const string ContainerName = "GeneratedTiles";

    public int Width => level.width;
    public int Depth => level.depth;

    void Start()
    {
        if (GameSession.selectedLevel != null) level = GameSession.selectedLevel;
        Build();
    }

    public void Build()
    {
        if (level == null) { Debug.LogWarning("GridManager has no LevelData assigned."); return; }
        if (tileSet == null || tileSet.fallbackPrefab == null)
        {
            Debug.LogWarning("GridManager needs a TileSet with a fallback prefab.");
            return;
        }
        if (!level.HasValidArrays)
        {
            Debug.LogWarning(level.name + " has no board yet. Select it and press Apply Layout.");
            return;
        }

        Clear();

        var container = new GameObject(ContainerName);
        container.transform.SetParent(transform, false);
        // In edit mode, keep generated tiles out of the saved scene file
        if (!Application.isPlaying) container.hideFlags = HideFlags.DontSave;

        var block = new MaterialPropertyBlock();

        for (int y = 0; y < level.depth; y++)
        for (int x = 0; x < level.width; x++)
        {
            var type = level.tileTypes[level.Index(x, y)];
            float top = (level.tileHeights[level.Index(x, y)] + 1) * heightStep;

            var tile = Instantiate(tileSet.GetPrefab(type), container.transform);
            tile.name = "Tile_" + x + "_" + y + "_" + type;
            tile.transform.localScale = new Vector3(1, top, 1);
            tile.transform.localPosition = new Vector3(x, top / 2f, y);

            var entry = tileSet.GetEntry(type);
            if (entry != null && entry.tint) Tint(tile, entry.color, block);
        }
    }

    // Colours a tile without creating a new material for each one
    static void Tint(GameObject tile, Color color, MaterialPropertyBlock block)
    {
        block.SetColor("_BaseColor", color);   // URP and HDRP shaders
        block.SetColor("_Color", color);       // built-in render pipeline shaders
        foreach (var r in tile.GetComponentsInChildren<Renderer>())
            r.SetPropertyBlock(block);
    }

    void Clear()
    {
        var old = transform.Find(ContainerName);
        if (old == null) return;
        if (Application.isPlaying) Destroy(old.gameObject);
        else DestroyImmediate(old.gameObject);
    }

    public bool InBounds(Vector2Int p) =>
        p.x >= 0 && p.y >= 0 && p.x < level.width && p.y < level.depth;

    public int GetHeight(Vector2Int p) => level.tileHeights[level.Index(p.x, p.y)];

    public TileType GetTileType(Vector2Int p) => level.tileTypes[level.Index(p.x, p.y)];

    public bool IsWalkable(Vector2Int p)
    {
        var t = GetTileType(p);
        return t != TileType.Water && t != TileType.Wall;
    }

    public Vector3 TileToWorld(Vector2Int p) =>
        transform.position + new Vector3(p.x, (GetHeight(p) + 1) * heightStep, p.y);

    // Shows spawn markers in the Scene view: blue = player, red = enemy
    void OnDrawGizmos()
    {
        if (level == null || level.spawns == null || !level.HasValidArrays) return;
        foreach (var s in level.spawns)
        {
            if (s.position.x < 0 || s.position.y < 0 || s.position.x >= level.width || s.position.y >= level.depth) continue;
            Gizmos.color = s.team == Team.Player ? Color.blue : Color.red;
            float h = (level.tileHeights[level.Index(s.position.x, s.position.y)] + 1) * heightStep;
            Gizmos.DrawSphere(transform.position + new Vector3(s.position.x, h + 0.4f, s.position.y), 0.25f);
        }
    }
}
