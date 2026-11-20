using System.Collections.Generic;
using UnityEngine;

// Plays the hurt and skill animations. Moving, attacking and dying are driven by the bool parameters elsewhere.
// A trigger is only set when the unit's controller has it, so older controllers keep working.
public class UnitAnimationEvents : MonoBehaviour
{
    readonly Dictionary<Animator, HashSet<string>> parameters = new Dictionary<Animator, HashSet<string>>();

    void OnEnable()
    {
        GameEvents.OnUnitDamaged += OnDamaged;
        GameEvents.OnSkillUsed += OnSkillUsed;
    }

    void OnDisable()
    {
        GameEvents.OnUnitDamaged -= OnDamaged;
        GameEvents.OnSkillUsed -= OnSkillUsed;
    }

    void OnDamaged(Unit unit)
    {
        if (unit != null && unit.currentHP > 0) Trigger(unit, "Hurt");
    }

    void OnSkillUsed(Unit caster, SkillData skill, List<Vector2Int> tiles)
    {
        if (caster == null) return;

        // Face the middle of the area, unless the skill is centred on the caster
        var grid = GridManager.Instance;
        if (skill.targeting == SkillTargeting.Tile && tiles.Count > 0)
        {
            Vector3 middle = Vector3.zero;
            foreach (var p in tiles) middle += grid.TileToWorld(p);
            middle /= tiles.Count;

            Vector3 dir = middle - caster.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) caster.transform.rotation = Quaternion.LookRotation(dir);
        }

        Trigger(caster, skill.castAnimation.ToString());
    }

    void Trigger(Unit unit, string name)
    {
        var anim = unit.GetComponentInChildren<Animator>();
        if (anim == null || !Has(anim, name)) return;
        anim.SetTrigger(name);
    }

    bool Has(Animator anim, string name)
    {
        if (!parameters.TryGetValue(anim, out var names))
        {
            names = new HashSet<string>();
            foreach (var p in anim.parameters) names.Add(p.name);
            parameters[anim] = names;
        }
        return names.Contains(name);
    }
}
