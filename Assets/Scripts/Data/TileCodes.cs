// Maps the letters used in a level's text layout to tile types and back.
// To add a tile type: add it to the TileType enum, then add its letter in both methods below.
public static class TileCodes
{
    public static bool TryGetType(char code, out TileType type)
    {
        switch (char.ToLower(code))
        {
            case 'g': type = TileType.Grass; return true;
            case 'd': type = TileType.Dirt;  return true;
            case 's': type = TileType.Stone; return true;
            case 'w': type = TileType.Water; return true;
            case '#': type = TileType.Wall;  return true;
            default:  type = TileType.Grass; return false;
        }
    }

    public static char GetCode(TileType type)
    {
        switch (type)
        {
            case TileType.Dirt:  return 'd';
            case TileType.Stone: return 's';
            case TileType.Water: return 'w';
            case TileType.Wall:  return '#';
            default:             return 'g';
        }
    }
}
