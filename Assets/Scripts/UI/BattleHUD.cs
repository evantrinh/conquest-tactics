using UnityEngine;
using UnityEngine.SceneManagement;

// A temporary interface drawn with Unity's built-in OnGUI, so it needs no Canvas.
// It only listens to GameEvents and BattleManager, so a proper Unity UI can replace this file whole.
public class BattleHUD : MonoBehaviour
{
    public static BattleHUD Instance { get; private set; }

    public string menuSceneName = "Menu";

    Unit selected;
    string result;
    Rect topBar, panel;

    float Scale => Mathf.Max(1f, Screen.height / 720f);

    void Awake() { Instance = this; }

    void OnEnable()
    {
        GameEvents.OnUnitSelected += OnSelected;
        GameEvents.OnSelectionCleared += OnCleared;
        GameEvents.OnLevelEnded += OnEnded;
    }

    void OnDisable()
    {
        GameEvents.OnUnitSelected -= OnSelected;
        GameEvents.OnSelectionCleared -= OnCleared;
        GameEvents.OnLevelEnded -= OnEnded;
    }

    void OnSelected(Unit unit) { selected = unit; }
    void OnCleared() { selected = null; }
    void OnEnded(bool playerWon) { result = playerWon ? "Victory" : "Defeat"; }

    // BattleInput asks this so that clicks on the HUD do not also click the board
    public bool BlocksPoint(Vector2 mouse)
    {
        float s = Scale;
        var p = new Vector2(mouse.x / s, (Screen.height - mouse.y) / s);
        return topBar.Contains(p) || (selected != null && panel.Contains(p));
    }

    void OnGUI()
    {
        float s = Scale;
        GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
        float w = Screen.width / s;
        float h = Screen.height / s;
        var battle = BattleManager.Instance;

        // Top bar
        topBar = new Rect(0, 0, w, 44);
        GUI.Box(topBar, GUIContent.none);
        GUILayout.BeginArea(new Rect(10, 6, w - 20, 32));
        GUILayout.BeginHorizontal();

        string who = result != null ? result : battle == null ? "" : (battle.CurrentTeam == Team.Player ? "Your turn" : "Enemy turn");
        string turn = battle == null ? "" : "   (turn " + battle.TurnNumber + ")";
        string ap = battle == null || battle.CurrentTeam != Team.Player ? "" :
            "   AP " + battle.Points.Remaining + " / " + battle.Points.Max;
        GUILayout.Label(who + turn + ap, GUILayout.Width(260));
        GUILayout.FlexibleSpace();

        bool busy = BattleInput.Instance != null && BattleInput.Instance.IsMoving;
        GUI.enabled = battle != null && battle.CurrentTeam == Team.Player && !busy;
        if (GUILayout.Button("End Turn", GUILayout.Width(110))) battle.EndTurn();
        GUI.enabled = true;
        if (GUILayout.Button("Menu", GUILayout.Width(80))) SceneManager.LoadScene(menuSceneName);

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Selected unit panel
        if (selected != null && selected.data != null)
        {
            panel = new Rect(10, h - 260, 270, 250);
            GUI.Box(panel, "Selected unit");
            GUILayout.BeginArea(new Rect(panel.x + 12, panel.y + 28, panel.width - 24, panel.height - 34));
            GUILayout.Label(selected.data.displayName + "  (" + selected.team + ")");
            GUILayout.Label("HP  " + selected.currentHP + " / " + UnitStats.MaxHP(selected));
            GUILayout.Label("Attack " + UnitStats.Attack(selected) + "   Move " + UnitStats.MoveRange(selected) +
                "   Range " + UnitStats.AttackRange(selected));
            bool ours = selected.team == Team.Player;
            if (battle != null && ours)
            {
                int moveCost = battle.MoveCost(selected);
                string move = !battle.CanMove(selected) ? "No move" : "Move " + (moveCost == 0 ? "free" : moveCost + " AP");
                string act = !battle.CanAct(selected) ? "No action" : "Attack " + battle.ActionCost(selected) + " AP";
                GUILayout.Label(move + "   " + act);
            }
            var skill = UnitStats.Skill(selected);
            if (skill != null && battle != null)
                GUILayout.Label("SP " + battle.SkillPoints.Get(selected) + " / " + battle.SkillPoints.Cap + "   " + skill.displayName + " costs " + skill.spCost);

            bool playerTurn = ours && battle != null && battle.CurrentTeam == Team.Player && !busy && !battle.LevelOver;
            var input = BattleInput.Instance;
            GUI.enabled = playerTurn;
            if (ours && skill != null)
            {
                if (input.IsTargeting)
                {
                    if (GUILayout.Button("Cancel skill")) input.CancelTargeting();
                }
                else
                {
                    GUI.enabled = playerTurn && battle.CanCast(selected, skill);
                    if (GUILayout.Button(skill.displayName)) input.BeginTargeting();
                    GUI.enabled = playerTurn;
                }
            }
            if (ours && GUILayout.Button("Wait")) input.WaitSelected();
            GUI.enabled = true;
            GUILayout.EndArea();
        }

        GUI.matrix = Matrix4x4.identity;
    }
}
