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

    [Tooltip("Turn this off once enemy AI ends its own turn by calling EndTurn().")]
    public bool autoSkipEnemyTurn = true;
    public float enemyTurnDelay = 1f;

    readonly List<Unit> units = new List<Unit>();
    readonly HashSet<Unit> moved = new HashSet<Unit>();

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

    public bool CanSelect(Unit unit) =>
        unit != null && unit.team == CurrentTeam && CurrentTeam == Team.Player && !moved.Contains(unit);

    public void MarkMoved(Unit unit) { moved.Add(unit); }

    public void EndTurn()
    {
        moved.Clear();
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
