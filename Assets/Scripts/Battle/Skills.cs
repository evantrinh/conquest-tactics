using System.Collections.Generic;
using UnityEngine;

// Skill rules. Where a skill can be aimed, what it covers, who it hits and for how much.
public static class Skills
{
    // Tiles the caster could pick as the centre from the origin tile
    public static List<Vector2Int> CastTiles(IGrid grid, SkillData skill, Vector2Int origin)
    {
        var tiles = new List<Vector2Int>();
        if (skill.targeting == SkillTargeting.Self || skill.castRange == null)
        {
            tiles.Add(origin);
            return tiles;
        }
        foreach (var o in skill.castRange.GetOffsets())
        {
            var p = origin + o;
            if (grid.InBounds(p)) tiles.Add(p);
        }
        return tiles;
    }

    public static List<Vector2Int> AreaTiles(IGrid grid, SkillData skill, Vector2Int center)
    {
        var tiles = new List<Vector2Int>();
        if (skill.area == null)
        {
            tiles.Add(center);
            return tiles;
        }
        foreach (var o in skill.area.GetOffsets())
        {
            var p = center + o;
            if (grid.InBounds(p)) tiles.Add(p);
        }
        return tiles;
    }

    // Units the skill would affect. The caster is treated as standing on casterPos.
    public static List<Unit> Affected(IGrid grid, IReadOnlyList<Unit> all, Unit caster,
        Vector2Int casterPos, SkillData skill, Vector2Int center)
    {
        var hit = new List<Unit>();
        if (skill.effect == SkillEffect.GrantAP)
        {
            hit.Add(caster);
            return hit;
        }

        foreach (var p in AreaTiles(grid, skill, center))
        foreach (var u in all)
        {
            if (u == null || u.currentHP <= 0 || hit.Contains(u)) continue;
            var pos = u == caster ? casterPos : u.gridPos;
            if (pos != p) continue;

            bool ally = u.team == caster.team;
            if (skill.effect == SkillEffect.Damage && !ally) hit.Add(u);
            if (skill.effect == SkillEffect.Heal && ally) hit.Add(u);
        }
        return hit;
    }

    public static List<Unit> Affected(IGrid grid, BattleManager battle, Unit caster, SkillData skill, Vector2Int center) =>
        Affected(grid, battle.Units, caster, caster.gridPos, skill, center);

    public static int Amount(IGrid grid, Unit caster, Vector2Int from, SkillData skill, Unit target)
    {
        int amount = skill.amount + Mathf.RoundToInt(UnitStats.Attack(caster) * skill.attackScale);
        if (skill.effect == SkillEffect.Damage) amount += Combat.HeightBonus(grid, from, target.gridPos);
        return Mathf.Max(0, amount);
    }

    // True while some centre the caster can pick from where it stands would do something
    public static bool HasUsefulTarget(IGrid grid, BattleManager battle, Unit caster, SkillData skill)
    {
        foreach (var center in CastTiles(grid, skill, caster.gridPos))
        foreach (var u in Affected(grid, battle, caster, skill, center))
        {
            if (skill.effect == SkillEffect.Heal && u.currentHP >= UnitStats.MaxHP(u)) continue;
            return true;
        }
        return false;
    }
}
