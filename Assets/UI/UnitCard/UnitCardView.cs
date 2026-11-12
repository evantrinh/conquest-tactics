using ContourUI;
using UnityEngine;
using UnityEngine.UIElements;

// Fills the unit card from the selected unit. Selection events show and hide it,
// then it reads the battle state every frame so the numbers and buttons stay current.
[RequireComponent(typeof(UIDocument))]
public class UnitCardView : MonoBehaviour
{
    VisualElement card, skillBlock, buttons;
    Label nameLabel, teamLabel, spLabel, attackStat, moveStat, rangeStat, statusLabel, skillName, skillDescription;
    HpBar hpBar;
    Pips spPips;
    Button skillButton, waitButton;
    Unit shown;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        card = root.Q<VisualElement>("unitCard");
        skillBlock = root.Q<VisualElement>("skillBlock");
        buttons = root.Q<VisualElement>("buttons");
        nameLabel = root.Q<Label>("unitName");
        teamLabel = root.Q<Label>("unitTeam");
        spLabel = root.Q<Label>("spLabel");
        attackStat = root.Q<Label>("attackStat");
        moveStat = root.Q<Label>("moveStat");
        rangeStat = root.Q<Label>("rangeStat");
        statusLabel = root.Q<Label>("statusLabel");
        skillName = root.Q<Label>("skillName");
        skillDescription = root.Q<Label>("skillDescription");
        hpBar = root.Q<HpBar>("hpBar");
        spPips = root.Q<Pips>("spPips");
        skillButton = root.Q<Button>("skillButton");
        waitButton = root.Q<Button>("waitButton");

        skillButton.clicked += OnSkillClicked;
        waitButton.clicked += OnWaitClicked;
        GameEvents.OnUnitSelected += Show;
        GameEvents.OnSelectionCleared += Hide;
    }

    void OnDisable()
    {
        skillButton.clicked -= OnSkillClicked;
        waitButton.clicked -= OnWaitClicked;
        GameEvents.OnUnitSelected -= Show;
        GameEvents.OnSelectionCleared -= Hide;
    }

    void Show(Unit unit)
    {
        shown = unit;
        card.AddToClassList("unit-card--shown");
        Refresh();
    }

    void Hide()
    {
        shown = null;
        card.RemoveFromClassList("unit-card--shown");
    }

    void Update()
    {
        if (shown != null) Refresh();
    }

    void Refresh()
    {
        var unit = shown;
        var battle = BattleManager.Instance;
        var input = BattleInput.Instance;
        bool ours = unit.team == Team.Player;
        bool playerTurn = battle.CurrentTeam == Team.Player && !battle.LevelOver;
        var skill = UnitStats.Skill(unit);

        nameLabel.text = unit.data.displayName;
        teamLabel.text = unit.team.ToString();
        hpBar.SetHp(unit.currentHP, UnitStats.MaxHP(unit));

        int sp = battle.SkillPoints.Get(unit);
        spLabel.text = "SP " + sp + " / " + battle.SkillPoints.Cap;
        spPips.SetPips(sp, battle.SkillPoints.Cap);

        attackStat.text = "Attack " + UnitStats.Attack(unit);
        moveStat.text = "Move " + UnitStats.MoveRange(unit);
        rangeStat.text = "Range " + UnitStats.AttackRange(unit);

        statusLabel.EnableInClassList("hidden", !ours);
        if (ours)
        {
            int moveCost = battle.MoveCost(unit);
            string move = !battle.CanMove(unit) ? "No move" : "Move " + (moveCost == 0 ? "free" : moveCost + " AP");
            string act = !battle.CanAct(unit) ? "No attack" : "Attack " + battle.ActionCost(unit) + " AP";
            statusLabel.text = move + "   " + act;
        }

        skillBlock.EnableInClassList("hidden", skill == null);
        skillButton.EnableInClassList("hidden", skill == null);
        if (skill != null)
        {
            string cost = skill.spCost + " SP" + (skill.actionCost > 0 && ours ? "   " + skill.actionCost + " AP" : "");
            skillName.text = skill.displayName + "   " + cost;
            skillDescription.text = skill.description;
        }

        buttons.EnableInClassList("hidden", !ours);
        if (!ours) return;

        bool free = playerTurn && !input.IsMoving;
        if (skill != null)
        {
            skillButton.text = input.IsTargeting ? "Cancel" : skill.displayName;
            skillButton.SetEnabled(free && (input.IsTargeting || (input.IsSelecting && battle.CanCast(unit, skill))));
        }
        waitButton.SetEnabled(free && input.IsSelecting);
    }

    void OnSkillClicked()
    {
        var input = BattleInput.Instance;
        if (input.IsTargeting) input.CancelTargeting();
        else input.BeginTargeting();
    }

    void OnWaitClicked() => BattleInput.Instance.WaitSelected();
}
