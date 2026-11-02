using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData data;
    public Team team;
    public Vector2Int gridPos;
    public int currentHP;

    void Start()
    {
        if (data != null) currentHP = data.maxHP;  // the asset is a template, so copy the value
    }
}