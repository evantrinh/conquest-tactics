using UnityEngine;

[CreateAssetMenu(menuName = "Game/Unit Data")]
public class UnitData : ScriptableObject
{
    public string displayName;
    public int maxHP = 10;
    public int attack = 3;
    public int moveRange = 3;
    public int attackRange = 1;
    public GameObject modelPrefab;
    // add skills and stuff as needed later
}
