using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mouse control for the player: select a unit, see its ranges, click a blue tile to move.
// States: Idle -> Selected -> Moving -> Idle.
public class BattleInput : MonoBehaviour
{
    public static BattleInput Instance { get; private set; }

    public float moveDuration = 0.35f;
    public bool IsMoving => state == State.Moving;

    enum State { Idle, Selected, Moving }
    State state = State.Idle;
    Unit selected;
    Dictionary<Vector2Int, int> reachable;
    Camera cam;

    void Awake() { Instance = this; }
    void OnEnable() { GameEvents.OnTurnChanged += OnTurnChanged; }
    void OnDisable() { GameEvents.OnTurnChanged -= OnTurnChanged; }
    void OnTurnChanged(Team team) { if (state == State.Selected) Deselect(); }

    void Update()
    {
        if (state == State.Moving) return;
        if (state == State.Selected && Input.GetKeyDown(KeyCode.Escape)) { Deselect(); return; }
        if (!Input.GetMouseButtonDown(0)) return;

        var battle = BattleManager.Instance;
        var grid = GridManager.Instance;
        if (battle == null || grid == null || battle.CurrentTeam != Team.Player) return;
        if (BattleHUD.Instance != null && BattleHUD.Instance.BlocksPoint(Input.mousePosition)) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 500f))
        {
            Deselect();
            return;
        }

        var unit = hit.collider.GetComponentInParent<Unit>();
        if (unit != null)
        {
            if (unit == selected) Deselect();
            else if (battle.CanSelect(unit)) Select(unit);
            return;
        }

        var tile = hit.collider.GetComponentInParent<TileCoord>();
        if (tile != null && state == State.Selected &&
            reachable.ContainsKey(tile.pos) && tile.pos != selected.gridPos)
            StartCoroutine(MoveRoutine(selected, tile.pos));
        else
            Deselect();
    }

    void Select(Unit unit)
    {
        var grid = GridManager.Instance;
        selected = unit;
        state = State.Selected;

        // Tiles the unit can walk to. Other units block the way.
        var blocked = BattleManager.Instance.OccupiedTiles(unit);
        reachable = MovementRange.GetReachable(grid, unit.gridPos, unit.data.moveRange, blocked);

        var moveTiles = new List<Vector2Int>();
        foreach (var kv in reachable)
            if (kv.Key != unit.gridPos) moveTiles.Add(kv.Key);

        // Attack range is a diamond around the unit's current tile (display only for now)
        var attackTiles = new List<Vector2Int>();
        int r = unit.data.attackRange;
        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        {
            if (Mathf.Abs(dx) + Mathf.Abs(dy) > r || (dx == 0 && dy == 0)) continue;
            var p = unit.gridPos + new Vector2Int(dx, dy);
            if (grid.InBounds(p)) attackTiles.Add(p);
        }

        if (TileOverlay.Instance != null) TileOverlay.Instance.Show(moveTiles, attackTiles);
        GameEvents.OnUnitSelected?.Invoke(unit);
    }

    void Deselect()
    {
        if (selected == null) return;
        selected = null;
        reachable = null;
        state = State.Idle;
        if (TileOverlay.Instance != null) TileOverlay.Instance.Clear();
        GameEvents.OnSelectionCleared?.Invoke();
    }

    IEnumerator MoveRoutine(Unit unit, Vector2Int target)
    {
        state = State.Moving;
        if (TileOverlay.Instance != null) TileOverlay.Instance.Clear();

        var grid = GridManager.Instance;
        // Keep the unit's own vertical offset (a capsule sits higher than a flat model)
        float yOffset = unit.transform.position.y - grid.TileToWorld(unit.gridPos).y;
        Vector3 from = unit.transform.position;
        Vector3 to = grid.TileToWorld(target) + Vector3.up * yOffset;

        // A straight slide for now. Walking along the path comes later.
        for (float t = 0f; t < moveDuration; t += Time.deltaTime)
        {
            unit.transform.position = Vector3.Lerp(from, to, t / moveDuration);
            yield return null;
        }
        unit.transform.position = to;
        unit.gridPos = target;

        BattleManager.Instance.MarkMoved(unit);
        GameEvents.OnUnitMoved?.Invoke(unit, target);

        selected = null;
        reachable = null;
        state = State.Idle;
        GameEvents.OnSelectionCleared?.Invoke();
    }
}
