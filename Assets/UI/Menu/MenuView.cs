using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// The menu pages, built from the GameCatalog at runtime. Editing a CampaignData or LevelData asset
// changes the menu. Choosing a level fills in GameSession and loads the battle scene.
[RequireComponent(typeof(UIDocument))]
public class MenuView : MonoBehaviour
{
    public GameCatalog catalog;
    public string gameTitle = "Contour Tactics";
    public string battleSceneName = "Battle";

    VisualElement homePage, campaignPage, levelPage, campaignList, levelList;
    Label campaignDescription;
    CampaignData chosen;

    // The hook for unlock progress later. Everything is open for now.
    bool IsUnlocked(CampaignData campaign, int levelIndex) => true;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Label>("title").text = gameTitle;
        homePage = root.Q<VisualElement>("homePage");
        campaignPage = root.Q<VisualElement>("campaignPage");
        levelPage = root.Q<VisualElement>("levelPage");
        campaignList = root.Q<VisualElement>("campaignList");
        levelList = root.Q<VisualElement>("levelList");
        campaignDescription = root.Q<Label>("campaignDescription");

        root.Q<Button>("playButton").clicked += ShowCampaigns;
        root.Q<Button>("quitButton").clicked += Quit;
        root.Q<Button>("campaignBack").clicked += ShowHome;
        root.Q<Button>("levelBack").clicked += ShowCampaigns;

        ShowHome();
    }

    void Show(VisualElement page)
    {
        homePage.EnableInClassList("hidden", page != homePage);
        campaignPage.EnableInClassList("hidden", page != campaignPage);
        levelPage.EnableInClassList("hidden", page != levelPage);
    }

    void ShowHome() => Show(homePage);

    void ShowCampaigns()
    {
        campaignList.Clear();
        if (catalog == null || catalog.campaigns == null || catalog.campaigns.Length == 0)
            campaignList.Add(new Label("The GameCatalog is empty or not assigned."));
        else
            foreach (var c in catalog.campaigns)
            {
                if (c == null) continue;
                var campaign = c;
                var button = new Button(() => ShowLevels(campaign))
                {
                    text = string.IsNullOrEmpty(c.campaignName) ? c.name : c.campaignName
                };
                campaignList.Add(button);
            }
        Show(campaignPage);
    }

    void ShowLevels(CampaignData campaign)
    {
        chosen = campaign;
        campaignDescription.text = campaign.description;
        levelList.Clear();

        if (campaign.levels == null || campaign.levels.Length == 0)
            levelList.Add(new Label("This campaign has no levels yet."));
        else
            for (int i = 0; i < campaign.levels.Length; i++)
            {
                var level = campaign.levels[i];
                if (level == null) continue;

                var button = new Button(() => StartLevel(level))
                {
                    text = (i + 1) + ".  " + (string.IsNullOrEmpty(level.levelName) ? level.name : level.levelName)
                };
                button.SetEnabled(IsUnlocked(campaign, i));
                levelList.Add(button);
            }
        Show(levelPage);
    }

    void StartLevel(LevelData level)
    {
        GameSession.selectedCampaign = chosen;
        GameSession.selectedLevel = level;
        SceneManager.LoadScene(battleSceneName);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
