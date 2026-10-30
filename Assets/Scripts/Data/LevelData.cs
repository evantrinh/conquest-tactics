using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class SpawnEntry
{
    public UnitData unit;
    public Vector2Int position;
    public Team team;
}

[CreateAssetMenu(menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;

    [Header("Authoring: type the level here, then press Apply Layout")]
    [Tooltip("One row per line, cells separated by spaces. Each cell is a type letter plus a height digit, for example g1 or w0. The first line is y = 0.")]
    [TextArea(8, 16)] public string layout;

    public SpawnEntry[] spawns;
    // Person A adds the objective reference in week 2

    // Filled in by Apply Layout. The game reads these arrays, never the text.
    [HideInInspector] public int width;
    [HideInInspector] public int depth;
    [HideInInspector] public int[] tileHeights;      // index = y * width + x
    [HideInInspector] public TileType[] tileTypes;   // same indexing

    public int Index(int x, int y) => y * width + x;

    public bool HasValidArrays =>
        width > 0 && depth > 0 &&
        tileHeights != null && tileTypes != null &&
        tileHeights.Length == width * depth &&
        tileTypes.Length == width * depth;

    // Text -> arrays
    public void ApplyLayout()
    {
        var rows = new List<string[]>();
        foreach (var line in (layout ?? "").Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) continue;
            rows.Add(trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        if (rows.Count == 0)
        {
            Debug.LogWarning(name + ": the layout is empty.");
            return;
        }

        depth = rows.Count;
        width = 0;
        foreach (var r in rows) width = Mathf.Max(width, r.Length);

        tileHeights = new int[width * depth];
        tileTypes = new TileType[width * depth];   // every cell starts as Grass at height 0

        for (int y = 0; y < depth; y++)
        {
            if (rows[y].Length < width)
                Debug.LogWarning(name + ": row " + y + " has " + rows[y].Length + " cells, expected " + width + ". Missing cells become g0.");

            for (int x = 0; x < rows[y].Length; x++)
            {
                string token = rows[y][x];

                if (!TileCodes.TryGetType(token[0], out var type))
                    Debug.LogWarning(name + ": unknown tile letter '" + token[0] + "' at row " + y + ", column " + x + ". Using grass.");

                int height = 0;
                if (token.Length > 1 && char.IsDigit(token[1])) height = token[1] - '0';
                else Debug.LogWarning(name + ": cell '" + token + "' at row " + y + ", column " + x + " has no height digit. Using 0.");

                tileTypes[Index(x, y)] = type;
                tileHeights[Index(x, y)] = height;
            }
        }

        if (spawns != null)
            foreach (var s in spawns)
                if (s.position.x < 0 || s.position.y < 0 || s.position.x >= width || s.position.y >= depth)
                    Debug.LogWarning(name + ": a spawn at " + s.position + " is outside the " + width + " x " + depth + " board.");
    }

    // Arrays -> text. Useful after the painter tool exists, and for copying a level into a spreadsheet.
    public void ExportLayout()
    {
        if (!HasValidArrays) { Debug.LogWarning(name + ": there are no valid arrays to export."); return; }

        var sb = new StringBuilder();
        for (int y = 0; y < depth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(TileCodes.GetCode(tileTypes[Index(x, y)]));
                sb.Append(Mathf.Clamp(tileHeights[Index(x, y)], 0, 9));
                if (x < width - 1) sb.Append(' ');
            }
            if (y < depth - 1) sb.Append('\n');
        }
        layout = sb.ToString();
    }
}
