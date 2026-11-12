using UnityEngine;

// How units gain skill points. Points are per unit and live in the SkillPoints component.
[CreateAssetMenu(menuName = "Game/Skill Point Rules")]
public class SkillPointRules : ScriptableObject
{
    public int startingPoints = 2;
    [Tooltip("Gained by every unit at the start of its team's turn.")]
    public int gainPerTurn = 1;
    [Tooltip("Extra points for a unit that ends its turn without moving or acting.")]
    public int idleBonus = 2;
    public int killBonus = 1;
    public int cap = 6;
}
