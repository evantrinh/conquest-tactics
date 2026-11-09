using ContourUI;
using UnityEngine;
using UnityEngine.UIElements;

// Fills the unit card from the selected unit and slides it in. It only listens to GameEvents.
[RequireComponent(typeof(UIDocument))]
public class UnitCardView : MonoBehaviour
{
    VisualElement card;
    Label nameLabel, teamLabel, attackStat, moveStat, rangeStat;
    HpBar hpBar;
    Unit shown;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        card = root.Q<VisualElement>("unitCard");
        nameLabel = root.Q<Label>("unitName");
        teamLabel = root.Q<Label>("unitTeam");
        hpBar = root.Q<HpBar>("hpBar");
        attackStat = root.Q<Label>("attackStat");
        moveStat = root.Q<Label>("moveStat");
        rangeStat = root.Q<Label>("rangeStat");

        GameEvents.OnUnitSelected += Show;
        GameEvents.OnSelectionCleared += Hide;
        GameEvents.OnUnitDamaged += OnDamaged;
    }

    void OnDisable()
    {
        GameEvents.OnUnitSelected -= Show;
        GameEvents.OnSelectionCleared -= Hide;
        GameEvents.OnUnitDamaged -= OnDamaged;
    }

    void Show(Unit unit)
    {
        shown = unit;
        nameLabel.text = unit.data.displayName;
        teamLabel.text = unit.team.ToString();
        attackStat.text = "Attack " + UnitStats.Attack(unit);
        moveStat.text = "Move " + UnitStats.MoveRange(unit);
        rangeStat.text = "Range " + UnitStats.AttackRange(unit);
        hpBar.SetHp(unit.currentHP, UnitStats.MaxHP(unit));

        card.EnableInClassList("unit-card--player", unit.team == Team.Player);
        card.EnableInClassList("unit-card--enemy", unit.team == Team.Enemy);
        card.AddToClassList("unit-card--shown");
    }

    void Hide()
    {
        shown = null;
        card.RemoveFromClassList("unit-card--shown");
    }

    void OnDamaged(Unit unit)
    {
        if (unit == shown) hpBar.SetHp(unit.currentHP, UnitStats.MaxHP(unit));
    }
}
