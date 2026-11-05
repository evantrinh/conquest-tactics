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
    public Unit Selected => selected;

    public float lungeDuration = 0.25f;
    public float lungeDistance = 0.4f;

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
            if (state == State.Selected && unit.team != selected.team)
            {
                if (battle.CanAct(selected) && Combat.InAttackRange(grid, selected, unit.gridPos))
                    StartCoroutine(AttackRoutine(selected, unit));
                return;
            }
            if (unit == selected) Deselect();
            else if (battle.CanSelect(unit)) Select(unit);
            return;
        }

        var tile = hit.collider.GetComponentInParent<TileCoord>();
        if (tile != null && state == State.Selected && battle.CanMove(selected) &&
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
        reachable = BattleManager.Instance.CanMove(unit)
            ? MovementRange.GetReachable(grid, unit.gridPos, UnitStats.MoveRange(unit), blocked)
            : new Dictionary<Vector2Int, int>();

        var moveTiles = new List<Vector2Int>();
        foreach (var kv in reachable)
            if (kv.Key != unit.gridPos) moveTiles.Add(kv.Key);

        var attackTiles = BattleManager.Instance.CanAct(unit)
            ? Combat.AttackTiles(grid, unit)
            : new List<Vector2Int>();

        if (TileOverlay.Instance != null) TileOverlay.Instance.Show(moveTiles, attackTiles);
        GameEvents.OnUnitSelected?.Invoke(unit);
    }

    public void WaitSelected()
    {
        if (state != State.Selected) return;
        var unit = selected;
        Deselect();
        BattleManager.Instance.Wait(unit);
        BattleManager.Instance.EndTurnIfNoOptions();
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
        // Keep the unit's own vertical offset since a capsule sits higher than a flat model
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

        Finish(unit);
    }

    IEnumerator AttackRoutine(Unit attacker, Unit target)
    {
        state = State.Moving;
        if (TileOverlay.Instance != null) TileOverlay.Instance.Clear();

        Vector3 from = attacker.transform.position;
        Vector3 dir = target.transform.position - from;
        dir.y = 0f;
        Vector3 peak = from + dir.normalized * lungeDistance;
        float half = lungeDuration * 0.5f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            attacker.transform.position = Vector3.Lerp(from, peak, t / half);
            yield return null;
        }
        attacker.transform.position = peak;
        BattleManager.Instance.Attack(attacker, target);
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            attacker.transform.position = Vector3.Lerp(peak, from, t / half);
            yield return null;
        }
        attacker.transform.position = from;

        Finish(attacker);
    }

    // Keeps the unit selected while it still has a move or an attack, then checks for the end of the turn
    void Finish(Unit unit)
    {
        var battle = BattleManager.Instance;
        reachable = null;
        state = State.Idle;
        selected = null;
        GameEvents.OnSelectionCleared?.Invoke();
        if (unit != null && battle.CanSelect(unit) && Combat.HasOptions(GridManager.Instance, battle, unit))
            Select(unit);
        battle.EndTurnIfNoOptions();
    }
}
