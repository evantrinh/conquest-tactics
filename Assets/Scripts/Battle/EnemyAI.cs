using System.Collections.Generic;
using UnityEngine;

// Decides what one unit does with its turn. Pure rules, no scene state, and fully deterministic.
public static class EnemyAI
{
    const int FarAway = 999;

    public struct Plan
    {
        public Vector2Int moveTo;
        public Unit target;   // null when nothing can be hit this turn
    }

    struct Candidate
    {
        public Vector2Int tile;
        public Unit target;
        public bool kill;
        public int hp, cost, damage;
    }

    public static Plan Decide(IGrid grid, Unit self, IReadOnlyList<Unit> all)
    {
        var plan = new Plan { moveTo = self.gridPos };
        var blocked = new HashSet<Vector2Int>();
        var foes = new Dictionary<Vector2Int, Unit>();
        foreach (var u in all)
        {
            if (u == null || u.currentHP <= 0) continue;
            if (u != self) blocked.Add(u.gridPos);
            if (u.team != self.team) foes[u.gridPos] = u;
        }
        if (foes.Count == 0) return plan;

        var reach = MovementRange.GetReachable(grid, self.gridPos, UnitStats.MoveRange(self), blocked);

        // Prefer a kill, then the lowest HP, then the cheapest tile, then the most damage
        bool found = false;
        Candidate best = default;
        foreach (var kv in reach)
        foreach (var p in Combat.AttackTiles(grid, self, kv.Key))
        {
            if (!foes.TryGetValue(p, out var foe)) continue;
            int damage = Combat.Damage(grid, self, kv.Key, foe);
            var c = new Candidate
            {
                tile = kv.Key, target = foe, cost = kv.Value, damage = damage,
                hp = foe.currentHP, kill = damage >= foe.currentHP
            };
            if (!found || Better(c, best)) { best = c; found = true; }
        }
        if (found)
        {
            plan.moveTo = best.tile;
            plan.target = best.target;
            return plan;
        }

        plan.moveTo = Advance(grid, self, reach, foes);
        return plan;
    }

    static bool Better(Candidate a, Candidate b)
    {
        if (a.kill != b.kill) return a.kill;
        if (a.hp != b.hp) return a.hp < b.hp;
        if (a.cost != b.cost) return a.cost < b.cost;
        if (a.damage != b.damage) return a.damage > b.damage;
        if (a.tile.y != b.tile.y) return a.tile.y < b.tile.y;
        if (a.tile.x != b.tile.x) return a.tile.x < b.tile.x;
        if (a.target.gridPos.y != b.target.gridPos.y) return a.target.gridPos.y < b.target.gridPos.y;
        return a.target.gridPos.x < b.target.gridPos.x;
    }

    // Walks the route to the nearest foe and stops on the furthest free tile within reach
    static Vector2Int Advance(IGrid grid, Unit self,
        Dictionary<Vector2Int, int> reach, Dictionary<Vector2Int, Unit> foes)
    {
        var cost = MovementRange.GetReachable(grid, self.gridPos, FarAway, null, out var cameFrom);

        bool found = false;
        Vector2Int goal = default;
        foreach (var p in foes.Keys)
        {
            if (!cost.ContainsKey(p)) continue;
            if (!found || cost[p] < cost[goal] ||
                (cost[p] == cost[goal] && (p.y < goal.y || (p.y == goal.y && p.x < goal.x))))
            {
                goal = p;
                found = true;
            }
        }
        if (!found) return self.gridPos;

        // The route runs goal back to start, so the first reachable tile met is the furthest along
        var step = goal;
        while (step != self.gridPos)
        {
            if (reach.ContainsKey(step)) return step;
            step = cameFrom[step];
        }
        return self.gridPos;
    }
}
