using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plays the enemy turn one unit at a time, then ends it.
public class EnemyTurnRunner : MonoBehaviour
{
    public float moveDuration = 0.35f;
    public float lungeDuration = 0.25f;
    public float lungeDistance = 0.4f;
    public float pause = 0.4f;

    void Start()
    {
        // This runner ends the enemy turn itself, so the timer skip is not needed
        if (BattleManager.Instance != null) BattleManager.Instance.autoSkipEnemyTurn = false;
    }

    void OnEnable() { GameEvents.OnTurnChanged += OnTurnChanged; }
    void OnDisable() { GameEvents.OnTurnChanged -= OnTurnChanged; }

    void OnTurnChanged(Team team)
    {
        if (team == Team.Enemy) StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        var battle = BattleManager.Instance;
        var grid = GridManager.Instance;
        yield return new WaitForSeconds(pause);

        var enemies = new List<Unit>();
        foreach (var u in battle.Units)
            if (u != null && u.team == Team.Enemy) enemies.Add(u);

        foreach (var enemy in enemies)
        {
            if (battle.LevelOver) yield break;
            if (enemy == null || enemy.currentHP <= 0) continue;

            var plan = EnemyAI.Decide(grid, enemy, battle.Units, battle.SkillPoints.Get(enemy));

            if (plan.moveTo != enemy.gridPos)
            {
                yield return UnitMotion.Slide(enemy, plan.moveTo, moveDuration);
                battle.MarkMoved(enemy);
                GameEvents.OnUnitMoved?.Invoke(enemy, plan.moveTo);
            }

            if (plan.skill != null)
            {
                yield return SkillCast.Run(enemy, plan.skill, plan.skillCenter);
            }
            else if (plan.target != null && plan.target.currentHP > 0)
            {
                var target = plan.target;
                yield return UnitMotion.Lunge(enemy, target, lungeDuration, lungeDistance,
                    () => battle.Attack(enemy, target));
            }
            yield return new WaitForSeconds(pause);
        }

        if (!battle.LevelOver) battle.EndTurn();
    }
}
