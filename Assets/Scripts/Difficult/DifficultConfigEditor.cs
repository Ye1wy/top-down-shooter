#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DifficultConfig))]
public class DifficultConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("difficult"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("profile"), true);

        serializedObject.ApplyModifiedProperties();

        var config = (DifficultConfig)target;

        GUILayout.Space(12);

        EditorGUILayout.HelpBox(
            "Save Scene → Asset: снять расстановку объектов сцены в этот ассет.\n" +
            "Load Asset → Scene: применить сохранённый баланс к текущей сцене.",
            MessageType.Info);

        GUILayout.Space(6);

        GUI.backgroundColor = new Color(0.55f, 1f, 0.65f);
        if (GUILayout.Button("Save Scene → Asset", GUILayout.Height(40)))
            config.SaveSceneToAsset();

        GUI.backgroundColor = new Color(0.65f, 0.85f, 1f);
        if (GUILayout.Button("Load Asset → Scene", GUILayout.Height(40)))
            config.ApplyToScene();

        GUI.backgroundColor = Color.white;
    }
}
#endif
