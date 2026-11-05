using System.Collections.Generic;
using UnityEngine;

// Attack rules. Range and damage each live in one function.
public static class Combat
{
    public const int BonusPerLevel = 1;
    public const int MaxHeightBonus = 2;

    // Tiles the unit can hit from where it stands
    public static List<Vector2Int> AttackTiles(IGrid grid, Unit unit)
    {
        var tiles = new List<Vector2Int>();
        int r = UnitStats.AttackRange(unit);
        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        {
            if (Mathf.Abs(dx) + Mathf.Abs(dy) > r || (dx == 0 && dy == 0)) continue;
            var p = unit.gridPos + new Vector2Int(dx, dy);
            if (grid.InBounds(p)) tiles.Add(p);
        }
        return tiles;
    }

    public static bool InAttackRange(IGrid grid, Unit attacker, Vector2Int target) =>
        AttackTiles(grid, attacker).Contains(target);

    public static int Damage(IGrid grid, Unit attacker, Unit target)
    {
        int diff = grid.GetHeight(attacker.gridPos) - grid.GetHeight(target.gridPos);
        int bonus = Mathf.Clamp(diff * BonusPerLevel, 0, MaxHeightBonus);
        return UnitStats.Attack(attacker) + bonus;
    }

    public static bool HasEnemyInRange(IGrid grid, BattleManager battle, Unit unit)
    {
        foreach (var p in AttackTiles(grid, unit))
        {
            var other = battle.GetUnitAt(p);
            if (other != null && other.team != unit.team) return true;
        }
        return false;
    }

    // True while the unit still has a move that goes somewhere or an attack that hits something
    public static bool HasOptions(IGrid grid, BattleManager battle, Unit unit)
    {
        if (battle.CanMove(unit))
        {
            var reach = MovementRange.GetReachable(grid, unit.gridPos, UnitStats.MoveRange(unit),
                battle.OccupiedTiles(unit));
            if (reach.Count > 1) return true;
        }
        return battle.CanAct(unit) && HasEnemyInRange(grid, battle, unit);
    }
}
