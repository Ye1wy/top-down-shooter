#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DifficultConfig))]
public class DifficultConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        var config = (DifficultConfig)target;

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Save Current Level", GUILayout.Height(40)))
        {
            Undo.RecordObject(config, "Save Balance Config");
            config.SaveCurrentLevel();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Load Current Level", GUILayout.Height(40)))
        {
            config.LoadCurrentLevel();
        }

        GUI.backgroundColor = Color.white;
    }
}
#endif
