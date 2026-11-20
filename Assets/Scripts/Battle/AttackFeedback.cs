using System.Collections;
using UnityEngine;

// Hit feedback without animation: the target flashes white and a damage number floats up.
// Healing and action points get their own coloured numbers.
// The attacker lunge lives in BattleInput.
public class AttackFeedback : MonoBehaviour
{
    public float flashTime = 0.15f;
    public float numberTime = 0.8f;
    public float numberRise = 1f;

    public Color damageColor = Color.yellow;
    public Color healColor = new Color(0.4f, 1f, 0.5f);
    public Color actionPointColor = new Color(1f, 0.85f, 0.2f);

    void OnEnable()
    {
        GameEvents.OnUnitAttacked += OnAttacked;
        GameEvents.OnUnitHealed += OnHealed;
        GameEvents.OnSkillUsed += OnSkillUsed;
    }

    void OnDisable()
    {
        GameEvents.OnUnitAttacked -= OnAttacked;
        GameEvents.OnUnitHealed -= OnHealed;
        GameEvents.OnSkillUsed -= OnSkillUsed;
    }

    void OnHealed(Unit unit, int amount)
    {
        StartCoroutine(Number(unit.transform.position + Vector3.up * 1.2f, "+" + amount, healColor));
    }

    void OnSkillUsed(Unit caster, SkillData skill, System.Collections.Generic.List<Vector2Int> tiles)
    {
        if (skill.effect == SkillEffect.GrantAP) StartCoroutine(ApNumber(caster, skill.amount));
    }

    // Shows when the points land, not when the skill starts
    IEnumerator ApNumber(Unit caster, int points)
    {
        yield return new WaitForSeconds(SkillCast.Windup);
        if (caster != null)
            yield return Number(caster.transform.position + Vector3.up * 1.2f, "+" + points + " AP", actionPointColor);
    }

    void OnAttacked(Unit attacker, Unit target, int damage)
    {
        StartCoroutine(Flash(target));
        StartCoroutine(Number(target.transform.position + Vector3.up * 1.2f, damage.ToString(), damageColor));
    }

    IEnumerator Flash(Unit target)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", Color.white);
        block.SetColor("_Color", Color.white);
        var renderers = target.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.SetPropertyBlock(block);
        yield return new WaitForSeconds(flashTime);
        foreach (var r in renderers) if (r != null) r.SetPropertyBlock(null);
    }

    IEnumerator Number(Vector3 start, string label, Color color)
    {
        var go = new GameObject("DamageNumber");
        go.transform.position = start;
        var text = go.AddComponent<TextMesh>();
        text.text = label;
        text.characterSize = 0.15f;
        text.fontSize = 48;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = color;
        var cam = Camera.main;

        for (float t = 0f; t < numberTime; t += Time.deltaTime)
        {
            go.transform.position = start + Vector3.up * (numberRise * t / numberTime);
            if (cam != null) go.transform.rotation = cam.transform.rotation;
            yield return null;
        }
        Destroy(go);
    }
}
