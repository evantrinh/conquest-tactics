using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    public string displayName;
    public int maxHP = 10;
    public int attack = 3;
    public int moveRange = 3;
    public int attackRange = 1;
    [Tooltip("Optional. When empty the attack range number is used as a diamond.")]
    public RangePattern attackPattern;
    [Tooltip("Optional. The one skill this unit can use.")]
    public SkillData skill;
    public GameObject modelPrefab;
    // add skills and stuff as needed later
}
