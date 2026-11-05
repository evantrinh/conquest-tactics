using UnityEngine;

// The player team's shared pool of action points for the current turn.
// Only the amount spent is stored. The size of the pool is worked out when asked.
public class ActionPoints : MonoBehaviour
{
    [Tooltip("Optional. Leave empty to use the default rules.")]
    public ActionPointRules rules;

    public int Spent { get; private set; }

    public ActionPointRules Rules
    {
        get
        {
            if (rules == null) rules = ScriptableObject.CreateInstance<ActionPointRules>();
            return rules;
        }
    }

    public int Max
    {
        get
        {
            var grid = GridManager.Instance;
            if (grid != null && grid.level != null && grid.level.actionPointsOverride > 0)
                return grid.level.actionPointsOverride;

            int living = 0;
            var battle = BattleManager.Instance;
            if (battle != null)
                foreach (var u in battle.Units)
                    if (u != null && u.team == Team.Player) living++;
            return Mathf.CeilToInt(living * Rules.poolPerUnit) + Rules.flatBonus;
        }
    }

    public int Remaining => Mathf.Max(0, Max - Spent);

    public bool CanSpend(int cost) => Remaining >= cost;

    public void Spend(int cost)
    {
        if (cost <= 0) return;
        Spent += cost;
        GameEvents.OnActionPointsChanged?.Invoke(Remaining, Max);
    }

    public void Refill()
    {
        Spent = 0;
        GameEvents.OnActionPointsChanged?.Invoke(Remaining, Max);
    }
}
