using UnityEngine;

// How many action points the player team gets and what things cost.
[CreateAssetMenu(menuName = "Game/Action Point Rules")]
public class ActionPointRules : ScriptableObject
{
    [Tooltip("Pool size per living player unit, rounded up. A level can set its own number instead.")]
    public float poolPerUnit = 1.5f;
    public int flatBonus = 0;

    [Tooltip("Cost of a unit's first move each turn.")]
    public int firstMoveCost = 0;
    [Tooltip("Cost of every move after the first.")]
    public int extraMoveCost = 1;
    [Tooltip("Cost of an attack or a skill.")]
    public int actionCost = 1;
}
