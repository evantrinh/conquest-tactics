using System.Collections.Generic;
using UnityEngine;

// The tiles an attack can hit, as offsets from the attacker. Pick a preset or list the offsets by hand.
[CreateAssetMenu(menuName = "Game/Range Pattern")]
public class RangePattern : ScriptableObject
{
    public enum Preset { Custom, Diamond, Line, Ring, Cross }

    public Preset preset = Preset.Diamond;

    [Tooltip("Reach of the preset in tiles.")]
    public int size = 1;

    [Tooltip("Used when the preset is Custom.")]
    public List<Vector2Int> offsets = new List<Vector2Int>();

    public List<Vector2Int> GetOffsets() =>
        preset == Preset.Custom ? offsets : Generate(preset, size);

    public static List<Vector2Int> Diamond(int size) => Generate(Preset.Diamond, size);

    // Diamond: every tile within the distance. Line: straight out in four directions.
    // Ring: only the tiles at exactly the distance. Cross: diagonals out in four directions.
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
            }
            if (keep) list.Add(new Vector2Int(dx, dy));
        }
        return list;
    }
}
