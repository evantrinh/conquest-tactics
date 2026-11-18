using System;
using System.Collections;
using UnityEngine;

// Unit movement and attack animation shared by the player and the enemy AI.
public static class UnitMotion
{
    // If the model faces sideways after this change, try 90, -90 or 180 here.
    const float FacingOffset = 0f;
    const float TurnSpeed = 720f; // degrees per second

    static Quaternion FaceRotation(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return Quaternion.identity;
        return Quaternion.LookRotation(dir) * Quaternion.Euler(0f, FacingOffset, 0f);
    }

    // A straight slide to the tile. Sets the unit's grid position when it arrives.
    public static IEnumerator Slide(Unit unit, Vector2Int target, float duration)
    {
        var grid = GridManager.Instance;
        // Keep the unit's own vertical offset since a capsule sits higher than a flat model
        float yOffset = unit.transform.position.y - grid.TileToWorld(unit.gridPos).y;
        Vector3 from = unit.transform.position;
        Vector3 to = grid.TileToWorld(target) + Vector3.up * yOffset;

        Vector3 dir = to - from;
        dir.y = 0f;
        bool hasDir = dir.sqrMagnitude > 0.0001f;
        Quaternion faceRot = hasDir ? FaceRotation(dir) : unit.transform.rotation;

        var anim = unit.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool("IsMoving", true);

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            unit.transform.position = Vector3.Lerp(from, to, t / duration);
            if (hasDir)
                unit.transform.rotation = Quaternion.RotateTowards(
                    unit.transform.rotation, faceRot, TurnSpeed * Time.deltaTime);
            yield return null;
        }
        unit.transform.position = to;
        if (hasDir) unit.transform.rotation = faceRot;
        unit.gridPos = target;

        if (anim != null) anim.SetBool("IsMoving", false);
    }

    // Steps toward the target and back. onHit runs at the furthest point.
    public static IEnumerator Lunge(Unit attacker, Unit target, float duration, float distance, Action onHit)
    {
        Vector3 from = attacker.transform.position;
        Vector3 dir = target.transform.position - from;
        dir.y = 0f;
        Vector3 peak = from + dir.normalized * distance;
        float half = duration * 0.5f;

    // Face the target for the attack
        if (dir.sqrMagnitude > 0.0001f)
            attacker.transform.rotation = FaceRotation(dir);

        var anim = attacker.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool("IsAttacking", true);

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

        if (anim != null) anim.SetBool("IsAttacking", false);
    }
}