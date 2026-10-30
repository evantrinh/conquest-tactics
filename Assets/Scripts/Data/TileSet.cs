using System;
using UnityEngine;

// Looks up what each tile type looks like. A type with no prefab uses the fallback prefab.
[CreateAssetMenu(menuName = "Game/Tile Set")]
public class TileSet : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public TileType type;
        public GameObject prefab;        // leave empty to use the fallback prefab
        public bool tint = true;         // colour the prefab (turn off for textured models)
        public Color color = Color.white;
    }

    public GameObject fallbackPrefab;    // a plain cube
    public Entry[] entries;

    public Entry GetEntry(TileType type)
    {
        if (entries == null) return null;
        foreach (var e in entries)
            if (e.type == type) return e;
        return null;
    }

    public GameObject GetPrefab(TileType type)
    {
        var e = GetEntry(type);
        if (e != null && e.prefab != null) return e.prefab;
        return fallbackPrefab;
    }
}
