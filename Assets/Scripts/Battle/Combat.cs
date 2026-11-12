using System.Collections.Generic;
using UnityEngine;

// Attack rules. Range and damage each live in one function.
public static class Combat
{
    public const int BonusPerLevel = 1;
    public const int MaxHeightBonus = 2;

    // Tiles the unit can hit from where it stands
    public static List<Vector2Int> AttackTiles(IGrid grid, Unit unit) =>
        AttackTiles(grid, unit, unit.gridPos);

    // Tiles the unit could hit if it stood on the origin tile
    public static List<Vector2Int> AttackTiles(IGrid grid, Unit unit, Vector2Int origin)
    {
        var tiles = new List<Vector2Int>();
        foreach (var o in UnitStats.AttackOffsets(unit))
        {
            var p = origin + o;
            if (grid.InBounds(p)) tiles.Add(p);
        }
        return tiles;
    }

    public static bool InAttackRange(IGrid grid, Unit attacker, Vector2Int target) =>
        AttackTiles(grid, attacker).Contains(target);

    public static int Damage(IGrid grid, Unit attacker, Unit target) =>
        Damage(grid, attacker, attacker.gridPos, target);

    public static int Damage(IGrid grid, Unit attacker, Vector2Int from, Unit target)
    {
        return UnitStats.Attack(attacker) + HeightBonus(grid, from, target.gridPos);
    }

    public static int HeightBonus(IGrid grid, Vector2Int from, Vector2Int to)
    {
        int diff = grid.GetHeight(from) - grid.GetHeight(to);
        return Mathf.Clamp(diff * BonusPerLevel, 0, MaxHeightBonus);
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
        var skill = UnitStats.Skill(unit);
        if (skill != null && battle.CanCast(unit, skill) && Skills.HasUsefulTarget(grid, battle, unit, skill)) return true;
        return battle.CanAct(unit) && HasEnemyInRange(grid, battle, unit);
    }
}
