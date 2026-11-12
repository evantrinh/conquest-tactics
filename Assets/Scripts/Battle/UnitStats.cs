using System.Collections.Generic;
using UnityEngine;

// The one place rules read unit stats. Buffs and debuffs will modify values here later.
public static class UnitStats
{
    public static int Attack(Unit u) => u.data.attack;
    public static int MoveRange(Unit u) => u.data.moveRange;
    public static int AttackRange(Unit u) => u.data.attackRange;
    public static int MaxHP(Unit u) => u.data.maxHP;
    public static SkillData Skill(Unit u) => u.data.skill;

    public static List<Vector2Int> AttackOffsets(Unit u) =>
        u.data.attackPattern != null
            ? u.data.attackPattern.GetOffsets()
            : RangePattern.Diamond(AttackRange(u));
}
