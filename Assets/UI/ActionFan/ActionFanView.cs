using UnityEngine;
using UnityEngine.UIElements;

// Round buttons that fan out beside the selected player unit: its skill, wait, and close.
[RequireComponent(typeof(UIDocument))]
public class ActionFanView : MonoBehaviour
{
    VisualElement fan;
    Button skillButton, waitButton, closeButton;
    Label skillCost;
    Camera cam;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        fan = root.Q<VisualElement>("fan");
        skillButton = root.Q<Button>("skillButton");
        waitButton = root.Q<Button>("waitButton");
        closeButton = root.Q<Button>("closeButton");
        skillCost = root.Q<Label>("skillCost");

        skillButton.clicked += OnSkill;
        waitButton.clicked += OnWait;
        closeButton.clicked += OnClose;
    }

    void OnDisable()
    {
        skillButton.clicked -= OnSkill;
        waitButton.clicked -= OnWait;
        closeButton.clicked -= OnClose;
    }

    void Update()
    {
        var battle = BattleManager.Instance;
        var input = BattleInput.Instance;
        var unit = input == null ? null : input.Selected;

        bool show = unit != null && unit.team == Team.Player && battle.CurrentTeam == Team.Player &&
                    !battle.LevelOver && (input.IsSelecting || input.IsTargeting);
        fan.EnableInClassList("hidden", !show);
        if (!show) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        var at = RuntimePanelUtils.CameraTransformWorldToPanel(fan.panel, unit.transform.position + Vector3.up * 0.8f, cam);
        fan.style.left = at.x;
        fan.style.top = at.y;

        var skill = UnitStats.Skill(unit);
        skillButton.EnableInClassList("hidden", skill == null);
        skillCost.EnableInClassList("hidden", skill == null);
        if (skill != null)
        {
            skillCost.text = skill.actionCost + " AP";
            skillButton.SetEnabled(input.IsSelecting && battle.CanCast(unit, skill));
        }
        waitButton.SetEnabled(input.IsSelecting);
    }

    void OnSkill() => BattleInput.Instance.BeginTargeting();
    void OnWait() => BattleInput.Instance.WaitSelected();
    void OnClose() => BattleInput.Instance.Cancel();
}
