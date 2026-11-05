using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps the list of units, whose turn it is, and what each player unit has used this turn.
// Each unit gets any number of moves and one action. Moves and actions spend the shared action points.
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public Team CurrentTeam { get; private set; } = Team.Player;
    public int TurnNumber { get; private set; } = 1;

    [Tooltip("Turn this off once enemy AI ends its own turn by calling EndTurn.")]
    public bool autoSkipEnemyTurn = true;
    public float enemyTurnDelay = 1f;

    readonly List<Unit> units = new List<Unit>();
    readonly Dictionary<Unit, int> moves = new Dictionary<Unit, int>();
    readonly HashSet<Unit> acted = new HashSet<Unit>();
    readonly HashSet<Unit> waited = new HashSet<Unit>();

    public bool LevelOver { get; private set; }

    public IReadOnlyList<Unit> Units => units;

    public ActionPoints Points { get; private set; }

    void Awake()
    {
        Instance = this;
        Points = GetComponent<ActionPoints>();
        if (Points == null) Points = gameObject.AddComponent<ActionPoints>();
    }
    void OnEnable() { GameEvents.OnUnitSpawned += Register; }
    void OnDisable() { GameEvents.OnUnitSpawned -= Register; }
    void Start() { GameEvents.OnTurnChanged?.Invoke(CurrentTeam); }

    void Register(Unit unit)
    {
        if (!units.Contains(unit)) units.Add(unit);
    }

    public Unit GetUnitAt(Vector2Int p)
    {
        foreach (var u in units)
            if (u != null && u.gridPos == p) return u;
        return null;
    }

    public HashSet<Vector2Int> OccupiedTiles(Unit except = null)
    {
        var set = new HashSet<Vector2Int>();
        foreach (var u in units)
            if (u != null && u != except) set.Add(u.gridPos);
        return set;
    }

    public bool HasMoved(Unit unit) => moves.ContainsKey(unit);
    public bool HasActed(Unit unit) => acted.Contains(unit);

    // Only the player team spends action points
    public int MoveCost(Unit unit)
    {
        if (unit.team != Team.Player) return 0;
        return HasMoved(unit) ? Points.Rules.extraMoveCost : Points.Rules.firstMoveCost;
    }

    public int ActionCost(Unit unit) => unit.team == Team.Player ? Points.Rules.actionCost : 0;

    public bool CanMove(Unit unit) =>
        !LevelOver && !waited.Contains(unit) && (MoveCost(unit) == 0 || Points.CanSpend(MoveCost(unit)));

    public bool CanAct(Unit unit) =>
        !LevelOver && !waited.Contains(unit) && !acted.Contains(unit) &&
        (ActionCost(unit) == 0 || Points.CanSpend(ActionCost(unit)));

    public bool CanSelect(Unit unit) =>
        unit != null && unit.team == CurrentTeam && CurrentTeam == Team.Player &&
        (CanMove(unit) || CanAct(unit));

    public void MarkMoved(Unit unit)
    {
        Points.Spend(MoveCost(unit));
        moves[unit] = moves.TryGetValue(unit, out var n) ? n + 1 : 1;
    }

    public void Wait(Unit unit)
    {
        waited.Add(unit);
        GameEvents.OnUnitWaited?.Invoke(unit);
    }

    public void Attack(Unit attacker, Unit target)
    {
        int damage = Combat.Damage(GridManager.Instance, attacker, target);
        Points.Spend(ActionCost(attacker));
        acted.Add(attacker);
        GameEvents.OnUnitAttacked?.Invoke(attacker, target, damage);
        target.currentHP -= damage;
        GameEvents.OnUnitDamaged?.Invoke(target);
        if (target.currentHP <= 0) Kill(target);
    }

    void Kill(Unit unit)
    {
        units.Remove(unit);
        GameEvents.OnUnitKilled?.Invoke(unit);
        foreach (var c in unit.GetComponentsInChildren<Collider>()) c.enabled = false;
        Destroy(unit.gameObject, 0.4f);

        bool playersLeft = false, enemiesLeft = false;
        foreach (var u in units)
        {
            if (u.team == Team.Player) playersLeft = true; else enemiesLeft = true;
        }
        if (playersLeft && enemiesLeft) return;
        LevelOver = true;
        GameEvents.OnLevelEnded?.Invoke(playersLeft);
    }

    // Ends the player turn when no unit has a move or an attack left
    public void EndTurnIfNoOptions()
    {
        if (LevelOver || CurrentTeam != Team.Player) return;
        foreach (var u in units)
            if (u != null && u.team == Team.Player && Combat.HasOptions(GridManager.Instance, this, u)) return;
        EndTurn();
    }

    public void EndTurn()
    {
        if (LevelOver) return;
        moves.Clear();
        acted.Clear();
        waited.Clear();
        Points.Refill();
        if (CurrentTeam == Team.Player) CurrentTeam = Team.Enemy;
        else { CurrentTeam = Team.Player; TurnNumber++; }

        GameEvents.OnTurnChanged?.Invoke(CurrentTeam);

        if (CurrentTeam == Team.Enemy && autoSkipEnemyTurn)
            StartCoroutine(SkipEnemyTurn());
    }

    IEnumerator SkipEnemyTurn()
    {
        yield return new WaitForSeconds(enemyTurnDelay);
        EndTurn();
    }
}
