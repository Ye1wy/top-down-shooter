using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultConfig", menuName = "DifficultConfig")]
public class DifficultConfig : ScriptableObject
{
    public Difficult.DifficultType CurrentDifficult;

    [SerializeField] private AmmoPickup ammoPrefab;

    // JSON хранится прямо в ScriptableObject
    [SerializeField, TextArea(10, 30)]
    private string ammoJson;

    [ContextMenu(nameof(SaveCurrentAmmo))]
    public void SaveCurrentAmmo()
    {
        var saveData = LoadSaveData();

        var currentAmmo = new AmmoLevelData();
        currentAmmo.Find();

        // обновляем данные для текущей сложности
        var existing = saveData.Levels
            .FirstOrDefault(x => x.DifficultType == CurrentDifficult);

        if (existing != null)
        {
            existing.Positions = currentAmmo.Positions;
        }
        else
        {
            saveData.Levels.Add(new AmmoLevelEntry
            {
                DifficultType = CurrentDifficult,
                Positions = currentAmmo.Positions
            });
        }

        ammoJson = JsonUtility.ToJson(saveData, true);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log($"Ammo saved for difficulty: {CurrentDifficult}");
    }

    [ContextMenu(nameof(LoadCurrentAmmo))]
    public void LoadCurrentAmmo()
    {
        ClearCurrentAmmo();

        var saveData = LoadSaveData();

        var levelData = saveData.Levels
            .FirstOrDefault(x => x.DifficultType == CurrentDifficult);

        if (levelData == null)
        {
            Debug.LogWarning($"No ammo data for difficulty: {CurrentDifficult}");
            return;
        }

        var ammoParent = FindFirstObjectByType<AmmoParent>();

        foreach (var position in levelData.Positions)
        {
            var ammo = Instantiate(ammoPrefab, position, Quaternion.identity);

            if (ammoParent != null)
            {
                ammo.transform.SetParent(ammoParent.transform);
            }
        }

        Debug.Log($"Ammo loaded for difficulty: {CurrentDifficult}");
    }

    private SaveData LoadSaveData()
    {
        if (string.IsNullOrWhiteSpace(ammoJson))
        {
            return new SaveData();
        }

        return JsonUtility.FromJson<SaveData>(ammoJson) ?? new SaveData();
    }

    private void ClearCurrentAmmo()
    {
        foreach (var ammo in FindObjectsByType<AmmoPickup>(
                     FindObjectsInactive.Exclude,
                     FindObjectsSortMode.None))
        {
            DestroyImmediate(ammo.gameObject);
        }
    }

    [Serializable]
    public class SaveData
    {
        public List<AmmoLevelEntry> Levels = new();
    }

    [Serializable]
    public class AmmoLevelEntry
    {
        public Difficult.DifficultType DifficultType;
        public List<Vector3> Positions = new();
    }

    [Serializable]
    public class AmmoLevelData
    {
        [SerializeField] private List<Vector3> positions = new();

        public List<Vector3> Positions => positions;

        public void Find()
        {
            positions.Clear();

            positions.AddRange(
                FindObjectsByType<AmmoPickup>(
                        FindObjectsInactive.Exclude,
                        FindObjectsSortMode.None)
                    .Select(ammo => ammo.transform.position));
        }
    }
}