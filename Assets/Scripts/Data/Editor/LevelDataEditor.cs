using UnityEditor;
using UnityEngine;

// Must stay inside a folder named Editor so it is left out of the game build.
[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var level = (LevelData)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Layout (text to level)"))
        {
            Undo.RecordObject(level, "Apply Layout");
            level.ApplyLayout();
            EditorUtility.SetDirty(level);
        }
        if (GUILayout.Button("Export Layout (level to text)"))
        {
            Undo.RecordObject(level, "Export Layout");
            level.ExportLayout();
            EditorUtility.SetDirty(level);
        }

        EditorGUILayout.HelpBox(
            level.HasValidArrays
                ? "Board size: " + level.width + " x " + level.depth
                : "No board yet. Type a layout and press Apply Layout.",
            level.HasValidArrays ? MessageType.Info : MessageType.Warning);
    }
}
