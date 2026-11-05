// The one place rules read unit stats. Buffs and debuffs will modify values here later.
public static class UnitStats
{
    public static int Attack(Unit u) => u.data.attack;
    public static int MoveRange(Unit u) => u.data.moveRange;
    public static int AttackRange(Unit u) => u.data.attackRange;
    public static int MaxHP(Unit u) => u.data.maxHP;
}
