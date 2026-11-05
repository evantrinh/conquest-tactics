using System.Collections.Generic;
using UnityEngine;

// Creates a unit on every spawn tile of the loaded level.
// A UnitData with no model prefab gets a coloured capsule, so units exist before art does.
// When real units arrive, assign their prefabs on the UnitData assets. Nothing here changes.
public class UnitSpawner : MonoBehaviour
{
    public float capsuleScale = 0.5f;
    public Color playerColor = new Color(0.2f, 0.4f, 1f);
    public Color enemyColor = new Color(0.9f, 0.2f, 0.2f);

    readonly List<GameObject> spawned = new List<GameObject>();

    void OnEnable() { GameEvents.OnLevelBuilt += OnLevelBuilt; }
    void OnDisable() { GameEvents.OnLevelBuilt -= OnLevelBuilt; }

    void OnLevelBuilt(Bounds _)
    {
        foreach (var go in spawned) if (go != null) Destroy(go);
        spawned.Clear();

        var grid = GridManager.Instance;
        if (grid == null || grid.level.spawns == null) return;

        foreach (var s in grid.level.spawns)
        {
            if (s.unit == null) { Debug.LogWarning("A spawn entry has no UnitData."); continue; }
            if (!grid.InBounds(s.position)) { Debug.LogWarning("Spawn " + s.position + " is outside the board."); continue; }
            Spawn(grid, s);
        }
    }

    void Spawn(GridManager grid, SpawnEntry s)
    {
        GameObject go;
        float lift = 0f;

        if (s.unit.modelPrefab != null)
        {
            go = Instantiate(s.unit.modelPrefab);
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.localScale = Vector3.one * capsuleScale;
            lift = capsuleScale;   // a capsule is 2 units tall, so half its height rests on the tile

            var block = new MaterialPropertyBlock();
            var color = s.team == Team.Player ? playerColor : enemyColor;
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            go.GetComponent<Renderer>().SetPropertyBlock(block);
        }

        go.name = s.team + "_" + s.unit.displayName;
        go.transform.position = grid.TileToWorld(s.position) + Vector3.up * lift;

        // Mouse selection needs a collider
        if (go.GetComponentInChildren<Collider>() == null)
        {
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.5f, 0);
            col.height = 1f;
            col.radius = 0.3f;
        }

        var unit = go.GetComponent<Unit>();
        if (unit == null) unit = go.AddComponent<Unit>();
        unit.data = s.unit;
        unit.team = s.team;
        unit.gridPos = s.position;
        unit.currentHP = UnitStats.MaxHP(unit);

        spawned.Add(go);
        GameEvents.OnUnitSpawned?.Invoke(unit);
    }
}
