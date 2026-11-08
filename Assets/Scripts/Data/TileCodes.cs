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
            case '^': type = TileType.Snow;  return true;
            case 'i': type = TileType.Ice;   return true;
            case 'l': type = TileType.Lava;  return true;
            case '+': type = TileType.Sand;  return true;
            case 'm': type = TileType.Mud;   return true;
            case 'q': type = TileType.QuickSand; return true;
            default:  type = TileType.Grass; return false;
        }
    }

    public static char GetCode(TileType type)
    {
        switch (type)
        {
            case TileType.Grass: return 'g';
            case TileType.Dirt:  return 'd';
            case TileType.Stone: return 's';
            case TileType.Water: return 'w';
            case TileType.Wall:  return '#';
            case TileType.Snow:  return '^';
            case TileType.Ice:   return 'i';
            case TileType.Lava:  return 'l';
            case TileType.Sand:  return '+';
            case TileType.Mud:   return 'm';
            case TileType.QuickSand: return 'q';
            default:             return 'g';
        }
    }
}
