using UnityEngine;
using UnityEngine.SceneManagement;

// A temporary menu drawn with OnGUI, so the scene needs no Canvas.
// The menu is built from the GameCatalog at runtime, so editing a CampaignData
// or LevelData asset changes the menu with no scene edits.
// A proper Unity UI can replace this script. It only has to read the same catalog
// and fill in GameSession before loading the battle scene.
public class MenuController : MonoBehaviour
{
    public GameCatalog catalog;
    public string gameTitle = "Contour Tactics";
    public string battleSceneName = "Battle";

    enum Page { Home, Campaigns, Levels }
    Page page = Page.Home;
    CampaignData chosen;

    // The hook for unlock progress later. Everything is open for now.
    bool IsUnlocked(CampaignData campaign, int levelIndex) => true;

    void OnGUI()
    {
        float s = Mathf.Max(1f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
        float w = Screen.width / s;
        float h = Screen.height / s;
        const float bw = 380f;

        GUILayout.BeginArea(new Rect((w - bw) / 2f, h * 0.15f, bw, h * 0.8f));

        var title = new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        GUILayout.Label(gameTitle, title);
        GUILayout.Space(16);

        switch (page)
        {
            case Page.Home: DrawHome(); break;
            case Page.Campaigns: DrawCampaigns(); break;
            case Page.Levels: DrawLevels(); break;
        }

        GUILayout.EndArea();
        GUI.matrix = Matrix4x4.identity;
    }

    void DrawHome()
    {
        if (Button("Play")) page = Page.Campaigns;
        if (Button("Quit"))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    void DrawCampaigns()
    {
        GUILayout.Label("Choose a campaign");
        if (catalog == null || catalog.campaigns == null || catalog.campaigns.Length == 0)
            GUILayout.Label("The GameCatalog is empty or not assigned.");
        else
            foreach (var c in catalog.campaigns)
            {
                if (c == null) continue;
                if (Button(string.IsNullOrEmpty(c.campaignName) ? c.name : c.campaignName))
                {
                    chosen = c;
                    page = Page.Levels;
                }
            }
        GUILayout.Space(10);
        if (Button("Back")) page = Page.Home;
    }

    void DrawLevels()
    {
        if (chosen == null) { page = Page.Campaigns; return; }

        if (!string.IsNullOrEmpty(chosen.description)) GUILayout.Label(chosen.description);
        GUILayout.Space(6);

        if (chosen.levels == null || chosen.levels.Length == 0)
            GUILayout.Label("This campaign has no levels yet.");
        else
            for (int i = 0; i < chosen.levels.Length; i++)
            {
                var level = chosen.levels[i];
                if (level == null) continue;

                string label = (i + 1) + ".  " + (string.IsNullOrEmpty(level.levelName) ? level.name : level.levelName);
                GUI.enabled = IsUnlocked(chosen, i);
                if (Button(label)) StartLevel(chosen, level);
                GUI.enabled = true;
            }

        GUILayout.Space(10);
        if (Button("Back")) page = Page.Campaigns;
    }

    void StartLevel(CampaignData campaign, LevelData level)
    {
        GameSession.selectedCampaign = campaign;
        GameSession.selectedLevel = level;
        SceneManager.LoadScene(battleSceneName);
    }

    static bool Button(string text) => GUILayout.Button(text, GUILayout.Height(40));
}
