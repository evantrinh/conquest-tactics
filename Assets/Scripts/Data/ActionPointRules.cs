using UnityEngine;

// How many action points the player team gets and what things cost.
[CreateAssetMenu(menuName = "Game/Action Point Rules")]
public class ActionPointRules : ScriptableObject
{
    [Tooltip("Pool size per living player unit, rounded up. A level can set its own number instead.")]
    public float poolPerUnit = 0.75f;
    public int flatBonus = 0;

    [Tooltip("Cost of a unit's move. Each unit can move once per turn.")]
    public int moveCost = 0;
    [Tooltip("Cost of an attack or a skill.")]
    public int actionCost = 1;
}
