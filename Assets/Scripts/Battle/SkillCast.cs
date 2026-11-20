using System.Collections;
using UnityEngine;

// The timing of a skill, shared by the player and the enemy AI.
// The effect starts glowing, then the result lands, then a short pause.
public static class SkillCast
{
    public const float Windup = 0.45f;
    public const float Recover = 0.35f;

    public static IEnumerator Run(Unit caster, SkillData skill, Vector2Int center)
    {
        var battle = BattleManager.Instance;
        if (!battle.BeginCast(caster, skill, center)) yield break;
        yield return new WaitForSeconds(Windup);
        battle.ResolveCast(caster, skill, center);
        yield return new WaitForSeconds(Recover);
    }
}
