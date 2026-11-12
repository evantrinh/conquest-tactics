using System.Collections.Generic;
using UnityEngine;

// Skill points for every unit. Runtime state only, the gain rules sit in a SkillPointRules asset.
public class SkillPoints : MonoBehaviour
{
    [Tooltip("Optional. Leave empty to use the default rules.")]
    public SkillPointRules rules;

    readonly Dictionary<Unit, int> points = new Dictionary<Unit, int>();
    readonly HashSet<Unit> active = new HashSet<Unit>();
    bool started;

    public SkillPointRules Rules
    {
        get
        {
            if (rules == null) rules = ScriptableObject.CreateInstance<SkillPointRules>();
            return rules;
        }
    }

    public int Cap => Rules.cap;
    public int Get(Unit unit) => points.TryGetValue(unit, out var n) ? n : 0;
    public bool Has(Unit unit, int cost) => Get(unit) >= cost;

    public void Spend(Unit unit, int cost) => Set(unit, Get(unit) - cost);
    void Add(Unit unit, int amount) => Set(unit, Mathf.Min(Rules.cap, Get(unit) + amount));

    void Set(Unit unit, int value)
    {
        points[unit] = Mathf.Max(0, value);
        GameEvents.OnSkillPointsChanged?.Invoke(unit, points[unit]);
    }

    void OnEnable()
    {
        GameEvents.OnUnitSpawned += OnSpawned;
        GameEvents.OnUnitKilled += OnKilled;
        GameEvents.OnUnitMoved += OnMoved;
        GameEvents.OnUnitAttacked += OnAttacked;
        GameEvents.OnSkillUsed += OnSkillUsed;
        GameEvents.OnKillScored += OnKillScored;
        GameEvents.OnTurnChanged += OnTurnChanged;
    }

    void OnDisable()
    {
        GameEvents.OnUnitSpawned -= OnSpawned;
        GameEvents.OnUnitKilled -= OnKilled;
        GameEvents.OnUnitMoved -= OnMoved;
        GameEvents.OnUnitAttacked -= OnAttacked;
        GameEvents.OnSkillUsed -= OnSkillUsed;
        GameEvents.OnKillScored -= OnKillScored;
        GameEvents.OnTurnChanged -= OnTurnChanged;
    }

    void OnSpawned(Unit unit) => Set(unit, Mathf.Min(Rules.cap, Rules.startingPoints));
    void OnKilled(Unit unit) { points.Remove(unit); active.Remove(unit); }
    void OnMoved(Unit unit, Vector2Int _) => active.Add(unit);
    void OnAttacked(Unit attacker, Unit target, int damage) => active.Add(attacker);
    void OnSkillUsed(Unit caster, SkillData skill, List<Vector2Int> tiles) => active.Add(caster);
    void OnKillScored(Unit killer, Unit victim) { if (killer != null) Add(killer, Rules.killBonus); }

    // The team that just finished gives idle bonuses, the team that is starting gains its turn points
    void OnTurnChanged(Team starting)
    {
        var battle = BattleManager.Instance;
        if (battle == null) return;

        if (started)
        {
            var finished = starting == Team.Player ? Team.Enemy : Team.Player;
            foreach (var u in battle.Units)
                if (u != null && u.team == finished && !active.Contains(u)) Add(u, Rules.idleBonus);
            foreach (var u in battle.Units)
                if (u != null && u.team == starting) Add(u, Rules.gainPerTurn);
        }
        started = true;
        active.Clear();
    }
}
