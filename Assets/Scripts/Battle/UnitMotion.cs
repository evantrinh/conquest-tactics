using System;
using System.Collections;
using UnityEngine;

// Unit movement and attack animation shared by the player and the enemy AI.
public static class UnitMotion
{
    // A straight slide to the tile. Sets the unit's grid position when it arrives.
    public static IEnumerator Slide(Unit unit, Vector2Int target, float duration)
    {
        var grid = GridManager.Instance;
        // Keep the unit's own vertical offset since a capsule sits higher than a flat model
        float yOffset = unit.transform.position.y - grid.TileToWorld(unit.gridPos).y;
        Vector3 from = unit.transform.position;
        Vector3 to = grid.TileToWorld(target) + Vector3.up * yOffset;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            unit.transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        unit.transform.position = to;
        unit.gridPos = target;
    }

    // Steps toward the target and back. onHit runs at the furthest point.
    public static IEnumerator Lunge(Unit attacker, Unit target, float duration, float distance, Action onHit)
    {
        Vector3 from = attacker.transform.position;
        Vector3 dir = target.transform.position - from;
        dir.y = 0f;
        Vector3 peak = from + dir.normalized * distance;
        float half = duration * 0.5f;

        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            attacker.transform.position = Vector3.Lerp(from, peak, t / half);
            yield return null;
        }
        attacker.transform.position = peak;
        onHit?.Invoke();
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            attacker.transform.position = Vector3.Lerp(peak, from, t / half);
            yield return null;
        }
        attacker.transform.position = from;
    }
}
