using UnityEngine;

// A campaign is an ordered list of levels. Edit the asset and the menu changes with it.
[CreateAssetMenu(menuName = "Game/Campaign Data")]
public class CampaignData : ScriptableObject
{
    public string campaignName;
    [TextArea(2, 5)] public string description;
    public LevelData[] levels;
}
