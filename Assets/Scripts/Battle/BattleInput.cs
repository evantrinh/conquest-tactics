using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mouse control for the player: select a unit, see its ranges, click a blue tile to move.
// States: Idle -> Selected -> Moving -> Idle. Inspecting shows the ranges and stats of any other unit.
// Targeting is entered from the Skill button and picks the centre of a skill.
public class BattleInput : MonoBehaviour
{
    public static BattleInput Instance { get; private set; }

    public float moveDuration = 0.35f;
    public bool IsMoving => state == State.Moving;
    public Unit Selected => selected;
    public bool IsTargeting => state == State.Targeting;

    public float lungeDuration = 0.25f;
    public float lungeDistance = 0.4f;

    enum State { Idle, Selected, Inspecting, Targeting, Moving }
    State state = State.Idle;
    Unit selected;
    Dictionary<Vector2Int, int> reachable;
    Camera cam;
    SkillData skill;
    List<Vector2Int> castTiles;
    Vector2Int hoverCenter;

    void Awake() { Instance = this; }
    void OnEnable() { GameEvents.OnTurnChanged += OnTurnChanged; }
    void OnDisable() { GameEvents.OnTurnChanged -= OnTurnChanged; }
    void OnTurnChanged(Team team) { if (state == State.Selected || state == State.Inspecting || state == State.Targeting) Deselect(); }

    void Update()
    {
        if (state == State.Moving) return;
        if (state == State.Targeting) { UpdateTargeting(); return; }
        if ((state == State.Selected || state == State.Inspecting) && Input.GetKeyDown(KeyCode.Escape)) { Deselect(); return; }
        if (!Input.GetMouseButtonDown(0)) return;

        var battle = BattleManager.Instance;
        var grid = GridManager.Instance;
        if (battle == null || grid == null || battle.CurrentTeam != Team.Player) return;
        if (BattleHUD.Instance != null && BattleHUD.Instance.BlocksPoint(Input.mousePosition)) return;

        if (!Pick(battle, out var units, out var tile)) { Deselect(); return; }

        if (state == State.Selected && battle.CanAct(selected))
        {
            foreach (var u in units)
            {
                if (u.team == selected.team || !Combat.InAttackRange(grid, selected, u.gridPos)) continue;
                StartCoroutine(AttackRoutine(selected, u));
                return;
            }
        }

        if (units.Count > 0)
        {
            var unit = units[0];
            if (unit == selected) Deselect();
            else if (battle.CanSelect(unit)) Select(unit);
            else Inspect(unit);
            return;
        }

        if (tile != null && state == State.Selected && battle.CanMove(selected) &&
            reachable.ContainsKey(tile.pos) && tile.pos != selected.gridPos)
            StartCoroutine(MoveRoutine(selected, tile.pos));
        else
            Deselect();
    }

    // Every collider under the cursor, nearest first, so a unit in front never hides one behind it
    bool Pick(BattleManager battle, out List<Unit> units, out TileCoord tile)
    {
        units = new List<Unit>();
        tile = null;
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        var hits = Physics.RaycastAll(cam.ScreenPointToRay(Input.mousePosition), 500f);
        if (hits.Length == 0) return false;
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        foreach (var h in hits)
        {
            var u = h.collider.GetComponentInParent<Unit>();
            if (u != null) { if (!units.Contains(u)) units.Add(u); continue; }
            if (tile == null) tile = h.collider.GetComponentInParent<TileCoord>();
        }
        // Clicking the tile under a unit counts as clicking the unit
        if (tile != null)
        {
            var onTile = battle.GetUnitAt(tile.pos);
            if (onTile != null && !units.Contains(onTile)) units.Add(onTile);
        }
        return true;
    }

    // Called by the HUD button
    public void BeginTargeting()
    {
        var battle = BattleManager.Instance;
        if (state != State.Selected) return;
        var chosen = UnitStats.Skill(selected);
        if (chosen == null || !battle.CanCast(selected, chosen)) return;

        skill = chosen;
        state = State.Targeting;
        castTiles = Skills.CastTiles(GridManager.Instance, skill, selected.gridPos);
        hoverCenter = new Vector2Int(-1, -1);
        ShowTargeting(castTiles.Count == 1 ? castTiles[0] : hoverCenter);
    }

    public void CancelTargeting()
    {
        if (state != State.Targeting) return;
        var unit = selected;
        state = State.Selected;
        Select(unit);
    }

    void ShowTargeting(Vector2Int center)
    {
        var grid = GridManager.Instance;
        var area = castTiles.Contains(center) ? Skills.AreaTiles(grid, skill, center) : new List<Vector2Int>();
        if (TileOverlay.Instance != null) TileOverlay.Instance.ShowSkill(castTiles, area, skill.effectColor);
    }

    void UpdateTargeting()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { CancelTargeting(); return; }

        var battle = BattleManager.Instance;
        if (battle == null || battle.CurrentTeam != Team.Player) return;
        if (BattleHUD.Instance != null && BattleHUD.Instance.BlocksPoint(Input.mousePosition)) return;
        if (!Pick(battle, out var units, out var tile)) return;

        bool found = units.Count > 0 || tile != null;
        var center = units.Count > 0 ? units[0].gridPos : (tile != null ? tile.pos : hoverCenter);

        if (found && center != hoverCenter)
        {
            hoverCenter = center;
            ShowTargeting(center);
        }

        if (!Input.GetMouseButtonDown(0)) return;
        if (found && castTiles.Contains(center)) StartCoroutine(SkillRoutine(selected, skill, center));
        else CancelTargeting();
    }

    // Shows the ranges and stats of a unit that can not be commanded right now
    void Inspect(Unit unit)
    {
        var grid = GridManager.Instance;
        selected = unit;
        state = State.Inspecting;
        reachable = null;

        var moveTiles = new List<Vector2Int>();
        var reach = MovementRange.GetReachable(grid, unit.gridPos, UnitStats.MoveRange(unit),
            BattleManager.Instance.OccupiedTiles(unit));
        foreach (var kv in reach)
            if (kv.Key != unit.gridPos) moveTiles.Add(kv.Key);

        if (TileOverlay.Instance != null)
            TileOverlay.Instance.Show(moveTiles, Combat.AttackTiles(grid, unit), unit.team != Team.Player);
        GameEvents.OnUnitSelected?.Invoke(unit);
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

        yield return UnitMotion.Slide(unit, target, moveDuration);

        BattleManager.Instance.MarkMoved(unit);
        GameEvents.OnUnitMoved?.Invoke(unit, target);

        Finish(unit);
    }

    IEnumerator AttackRoutine(Unit attacker, Unit target)
    {
        state = State.Moving;
        if (TileOverlay.Instance != null) TileOverlay.Instance.Clear();

        yield return UnitMotion.Lunge(attacker, target, lungeDuration, lungeDistance,
            () => BattleManager.Instance.Attack(attacker, target));

        Finish(attacker);
    }

    IEnumerator SkillRoutine(Unit caster, SkillData used, Vector2Int center)
    {
        state = State.Moving;
        if (TileOverlay.Instance != null) TileOverlay.Instance.Clear();
        yield return SkillCast.Run(caster, used, center);
        Finish(caster);
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
