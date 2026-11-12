using System.Collections.Generic;
using ContourUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// The top bar, the roster and the result banner of the battle screen.
// BattleInput asks BlocksPoint so that clicks on the interface do not also click the board.
[RequireComponent(typeof(UIDocument))]
public class BattleScreenView : MonoBehaviour
{
    public static BattleScreenView Instance { get; private set; }

    public string menuSceneName = "Menu";

    VisualElement root, roster;
    Label turnLabel, apLabel, banner;
    Pips apPips;
    Button endTurnButton, menuButton;
    readonly Dictionary<Unit, UnitChip> chips = new Dictionary<Unit, UnitChip>();
    readonly List<Unit> gone = new List<Unit>();

    void Awake() { Instance = this; }

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        roster = root.Q<VisualElement>("roster");
        turnLabel = root.Q<Label>("turnLabel");
        apLabel = root.Q<Label>("apLabel");
        banner = root.Q<Label>("banner");
        apPips = root.Q<Pips>("apPips");
        endTurnButton = root.Q<Button>("endTurnButton");
        menuButton = root.Q<Button>("menuButton");

        endTurnButton.clicked += OnEndTurn;
        menuButton.clicked += OnMenu;
        GameEvents.OnUnitSpawned += OnSpawned;
        GameEvents.OnLevelEnded += OnLevelEnded;
    }

    void OnDisable()
    {
        endTurnButton.clicked -= OnEndTurn;
        menuButton.clicked -= OnMenu;
        GameEvents.OnUnitSpawned -= OnSpawned;
        GameEvents.OnLevelEnded -= OnLevelEnded;
    }

    // True when the pointer is over a part of the interface that takes clicks
    public bool BlocksPoint(Vector2 mouse)
    {
        var panel = root == null ? null : root.panel;
        if (panel == null) return false;
        var point = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(mouse.x, Screen.height - mouse.y));
        return panel.Pick(point) != null;
    }

    void OnSpawned(Unit unit)
    {
        if (unit.team != Team.Player || chips.ContainsKey(unit)) return;

        var chip = new UnitChip();
        chip.RegisterCallback<ClickEvent>(_ => BattleInput.Instance.Choose(unit));
        roster.Add(chip);
        chips[unit] = chip;
    }

    void OnLevelEnded(bool playerWon)
    {
        banner.text = playerWon ? "Victory" : "Defeat";
        banner.AddToClassList("banner--shown");
    }

    void OnEndTurn()
    {
        var battle = BattleManager.Instance;
        if (battle != null && battle.CurrentTeam == Team.Player) battle.EndTurn();
    }

    void OnMenu() => SceneManager.LoadScene(menuSceneName);

    void Update()
    {
        var battle = BattleManager.Instance;
        if (battle == null) return;
        var input = BattleInput.Instance;

        bool playerTurn = battle.CurrentTeam == Team.Player && !battle.LevelOver;
        turnLabel.text = (battle.CurrentTeam == Team.Player ? "Your turn" : "Enemy turn") + "   Turn " + battle.TurnNumber;
        apLabel.text = "AP " + battle.Points.Remaining + " / " + battle.Points.Max;
        apPips.SetPips(battle.Points.Remaining, battle.Points.Max);
        endTurnButton.SetEnabled(playerTurn && !input.IsMoving);

        gone.Clear();
        foreach (var pair in chips)
        {
            var unit = pair.Key;
            if (unit == null) { gone.Add(unit); continue; }

            string state = battle.CanSelect(unit) ? "Ready" : "Done";
            pair.Value.Show(unit.data.displayName, unit.currentHP, UnitStats.MaxHP(unit), state);
            pair.Value.SetSelected(input.Selected == unit);
        }
        foreach (var unit in gone)
        {
            chips[unit].RemoveFromHierarchy();
            chips.Remove(unit);
        }
    }
}
