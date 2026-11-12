using System.Collections.Generic;
using UnityEngine;

// The tiles an attack can hit, as offsets from the attacker. Pick a preset or list the offsets by hand.
[CreateAssetMenu(menuName = "Game/Range Pattern")]
public class RangePattern : ScriptableObject
{
    public enum Preset { Custom, Diamond, Line, Ring, Cross, Circle, Square }

    public Preset preset = Preset.Diamond;

    [Tooltip("Reach of the preset in tiles.")]
    public int size = 1;

    [Tooltip("Used when the preset is Custom.")]
    public List<Vector2Int> offsets = new List<Vector2Int>();

    [Tooltip("Adds the centre tile to the pattern.")]
    public bool includeCenter = false;

    public List<Vector2Int> GetOffsets()
    {
        var list = preset == Preset.Custom ? new List<Vector2Int>(offsets) : Generate(preset, size);
        if (includeCenter && !list.Contains(Vector2Int.zero)) list.Insert(0, Vector2Int.zero);
        return list;
    }

    public static List<Vector2Int> Diamond(int size) => Generate(Preset.Diamond, size);

    // Diamond: every tile within the distance. Line: straight out in four directions.
    // Ring: only the tiles at exactly the distance. Cross: diagonals out in four directions.
    // Circle: a round blob that reaches the distance. Square: every tile within the distance in both axes.
    static List<Vector2Int> Generate(Preset preset, int size)
    {
        var list = new List<Vector2Int>();
        for (int dx = -size; dx <= size; dx++)
        for (int dy = -size; dy <= size; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int dist = Mathf.Abs(dx) + Mathf.Abs(dy);
            bool keep = false;
            switch (preset)
            {
                case Preset.Diamond: keep = dist <= size; break;
                case Preset.Ring: keep = dist == size; break;
                case Preset.Line: keep = dx == 0 || dy == 0; break;
                case Preset.Cross: keep = Mathf.Abs(dx) == Mathf.Abs(dy); break;
                case Preset.Circle: keep = dx * dx + dy * dy <= size * size + size; break;
                case Preset.Square: keep = true; break;
            }
            if (keep) list.Add(new Vector2Int(dx, dy));
        }
        return list;
    }
}
