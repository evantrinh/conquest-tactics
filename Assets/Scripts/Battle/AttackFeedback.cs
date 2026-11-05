using System.Collections;
using UnityEngine;

// Hit feedback without animation: the target flashes white and a damage number floats up.
// The attacker lunge lives in BattleInput.
public class AttackFeedback : MonoBehaviour
{
    public float flashTime = 0.15f;
    public float numberTime = 0.8f;
    public float numberRise = 1f;

    void OnEnable() { GameEvents.OnUnitAttacked += OnAttacked; }
    void OnDisable() { GameEvents.OnUnitAttacked -= OnAttacked; }

    void OnAttacked(Unit attacker, Unit target, int damage)
    {
        StartCoroutine(Flash(target));
        StartCoroutine(Number(target.transform.position + Vector3.up * 1.2f, damage));
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

    IEnumerator Number(Vector3 start, int damage)
    {
        var go = new GameObject("DamageNumber");
        go.transform.position = start;
        var text = go.AddComponent<TextMesh>();
        text.text = damage.ToString();
        text.characterSize = 0.15f;
        text.fontSize = 48;
        text.anchor = TextAnchor.MiddleCenter;
        text.color = Color.yellow;
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
