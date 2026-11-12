using ContourUI;
using UnityEngine;
using UnityEngine.UIElements;

// Fills the unit card from the selected unit. Selection events show and hide it,
// then it reads the battle state every frame so the numbers stay current.
[RequireComponent(typeof(UIDocument))]
public class UnitCardView : MonoBehaviour
{
    VisualElement card, skillBlock;
    Label portrait, nameLabel, subtitle, attackStat, moveStat, rangeStat, statusLabel, skillName, skillDescription;
    HpBar hpBar;
    SkillRing spRing;
    Unit shown;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        card = root.Q<VisualElement>("unitCard");
        skillBlock = root.Q<VisualElement>("skillBlock");
        portrait = root.Q<Label>("portrait");
        nameLabel = root.Q<Label>("unitName");
        subtitle = root.Q<Label>("unitSubtitle");
        attackStat = root.Q<Label>("attackStat");
        moveStat = root.Q<Label>("moveStat");
        rangeStat = root.Q<Label>("rangeStat");
        statusLabel = root.Q<Label>("statusLabel");
        skillName = root.Q<Label>("skillName");
        skillDescription = root.Q<Label>("skillDescription");
        hpBar = root.Q<HpBar>("hpBar");
        spRing = root.Q<SkillRing>("spRing");

        GameEvents.OnUnitSelected += Show;
        GameEvents.OnSelectionCleared += Hide;
    }

    void OnDisable()
    {
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
        bool ours = unit.team == Team.Player;
        var skill = UnitStats.Skill(unit);

        string unitName = unit.data.displayName;
        portrait.text = string.IsNullOrEmpty(unitName) ? "" : unitName.Substring(0, 1);
        nameLabel.text = unitName;
        subtitle.text = ours ? unit.team + "  " + (battle.CanSelect(unit) ? "Ready" : "Done") : unit.team.ToString();
        hpBar.SetHp(unit.currentHP, UnitStats.MaxHP(unit));
        spRing.SetPoints(battle.SkillPoints.Get(unit), battle.SkillPoints.Cap);

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
        if (skill != null)
        {
            string cost = skill.spCost + " SP" + (skill.actionCost > 0 && ours ? "   " + skill.actionCost + " AP" : "");
            skillName.text = skill.displayName + "   " + cost;
            skillDescription.text = skill.description;
        }
    }
}
