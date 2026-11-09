using UnityEngine;

// Zooms toward a unit when it is selected and returns to the overview when it is not.
// Swap this out or add other listeners to change what the camera does on selection.
public class SelectionCameraFocus : MonoBehaviour
{
    [Range(0.2f, 1f)] public float zoom = 0.65f;

    void OnEnable()
    {
        GameEvents.OnUnitSelected += OnSelected;
        GameEvents.OnSelectionCleared += OnCleared;
    }

    void OnDisable()
    {
        GameEvents.OnUnitSelected -= OnSelected;
        GameEvents.OnSelectionCleared -= OnCleared;
    }

    void OnSelected(Unit unit)
    {
        if (CameraRig.Instance != null) CameraRig.Instance.FocusOn(unit.transform.position, zoom);
    }

    void OnCleared()
    {
        if (CameraRig.Instance != null) CameraRig.Instance.ReturnToOverview();
    }
}
