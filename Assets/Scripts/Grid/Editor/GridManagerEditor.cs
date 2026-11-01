using UnityEditor;
using UnityEngine;

// Must stay inside a folder named Editor so it is left out of the game build.
[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Rebuild Level"))
            ((GridManager)target).Build();
    }
}
