using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps the list of units and whose turn it is. Barebones on purpose:
// the enemy turn is skipped until enemy AI exists.
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public Team CurrentTeam { get; private set; } = Team.Player;
    public int TurnNumber { get; private set; } = 1;

    [Tooltip("Turn this off once enemy AI ends its own turn by calling EndTurn.")]
    public bool autoSkipEnemyTurn = true;
    public float enemyTurnDelay = 1f;

    readonly List<Unit> units = new List<Unit>();
    readonly HashSet<Unit> moved = new HashSet<Unit>();
    readonly HashSet<Unit> acted = new HashSet<Unit>();

    public bool LevelOver { get; private set; }

    public IReadOnlyList<Unit> Units => units;

    void Awake() { Instance = this; }
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

    public bool HasMoved(Unit unit) => moved.Contains(unit);

    public bool HasActed(Unit unit) => acted.Contains(unit);
    public bool CanMove(Unit unit) => !LevelOver && !moved.Contains(unit);
    public bool CanAct(Unit unit) => !LevelOver && !acted.Contains(unit);

    public bool CanSelect(Unit unit) =>
        unit != null && unit.team == CurrentTeam && CurrentTeam == Team.Player &&
        (CanMove(unit) || CanAct(unit));

    public void MarkMoved(Unit unit) { moved.Add(unit); }
    public void MarkActed(Unit unit) { acted.Add(unit); }

    public void Wait(Unit unit)
    {
        moved.Add(unit);
        acted.Add(unit);
        GameEvents.OnUnitWaited?.Invoke(unit);
    }

    public void Attack(Unit attacker, Unit target)
    {
        int damage = Combat.Damage(GridManager.Instance, attacker, target);
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
        moved.Clear();
        acted.Clear();
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
