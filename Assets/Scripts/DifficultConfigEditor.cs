using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DifficultConfig))]
public class DifficultConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var config = (DifficultConfig)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("difficult"));
        serializedObject.ApplyModifiedProperties();

        serializedObject.Update();

        GUILayout.Space(8);

        string status = config.CurrentDifficultyHasSavedLevelBalance
            ? "Для выбранной сложности есть сохранённый профиль. Он накатан на панель ниже."
            : "Для выбранной сложности сохранённого профиля нет. На панель ниже накатаны дефолтные значения.";

        EditorGUILayout.HelpBox(status, MessageType.Info);

        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("profile"),
            new GUIContent($"{config.difficult} Settings Panel"),
            true);

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Save Current Level", GUILayout.Height(38)))
        {
            config.SaveCurrentLevel();
        }

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Load Current Level", GUILayout.Height(38)))
        {
            config.LoadCurrentLevel();
        }

        GUI.backgroundColor = new Color(0.65f, 0.85f, 1f);
        if (GUILayout.Button("Apply Panel To Scene", GUILayout.Height(32)))
        {
            config.ApplyPanelToCurrentScene();
        }

        GUI.backgroundColor = new Color(0.55f, 1f, 0.65f);
        if (GUILayout.Button("Save Panel To Selected Difficulty", GUILayout.Height(32)))
        {
            config.SavePanelToSelectedDifficulty();
        }

        GUI.backgroundColor = new Color(1f, 0.75f, 0.25f);
        if (GUILayout.Button("Reload Selected Difficulty To Panel", GUILayout.Height(32)))
        {
            config.LoadSelectedDifficultyToPanel();
        }

        GUI.backgroundColor = new Color(1f, 0.55f, 0.45f);
        if (GUILayout.Button("Reset Selected Difficulty To Defaults", GUILayout.Height(32)))
        {
            if (EditorUtility.DisplayDialog(
                    "Reset balance",
                    $"Reset {config.difficult} profile to default values? Saved scene object settings for this difficult will be cleared.",
                    "Reset",
                    "Cancel"))
            {
                config.ResetSelectedDifficultyToDefault();
            }
        }

        GUI.backgroundColor = Color.white;
    }
}
