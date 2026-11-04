using UnityEngine;

// The list of every campaign in the game. The menu reads this asset.
[CreateAssetMenu(menuName = "Game/Game Catalog")]
public class GameCatalog : ScriptableObject
{
    public CampaignData[] campaigns;
}
