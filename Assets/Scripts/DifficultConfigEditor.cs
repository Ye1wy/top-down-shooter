using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DifficultConfig))]
public class DifficultConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Рисуем стандартный инспектор
        DrawDefaultInspector();

        GUILayout.Space(10);

        var config = (DifficultConfig)target;

        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Save Current Level", GUILayout.Height(40)))
        {
            config.SaveCurrentLevel();

            EditorUtility.SetDirty(config);
        }

        GUI.backgroundColor = Color.cyan;

        if (GUILayout.Button("Load Current Level", GUILayout.Height(40)))
        {
            config.LoadCurrentLevel();
        }

        GUI.backgroundColor = Color.white;
    }
}
