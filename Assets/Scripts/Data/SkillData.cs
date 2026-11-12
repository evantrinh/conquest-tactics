using UnityEngine;

public enum SkillEffect { Damage, Heal, GrantAP }

public enum SkillTargeting { Tile, Self }

// One skill. Damage hits enemies in the area, Heal restores allies in the area, GrantAP adds action points for the turn.
[CreateAssetMenu(menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    public string displayName;
    public SkillEffect effect;
    public SkillTargeting targeting = SkillTargeting.Tile;

    [Tooltip("Skill points spent by the caster.")]
    public int spCost = 2;
    [Tooltip("Action points spent by a player unit.")]
    public int actionCost = 1;

    [Tooltip("Tiles the caster can pick as the centre. Empty means the caster's own tile.")]
    public RangePattern castRange;
    [Tooltip("Tiles hit around the centre. Empty means the centre tile only.")]
    public RangePattern area;

    [Tooltip("Flat damage, healing or action points.")]
    public int amount;
    [Tooltip("Share of the caster's attack added to the amount.")]
    public float attackScale;

    public Color effectColor = Color.white;
}
