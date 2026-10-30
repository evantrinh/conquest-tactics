using UnityEngine;

public interface IGrid
{
    int Width { get; }
    int Depth { get; }
    bool InBounds(Vector2Int p);
    int GetHeight(Vector2Int p);
    TileType GetTileType(Vector2Int p);
    bool IsWalkable(Vector2Int p);
}
