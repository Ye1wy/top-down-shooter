using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultConfig", menuName = "DifficultConfig")]
public class DifficultConfig : ScriptableObject
{
    public Difficult.DifficultType CurrentDifficult;

    [Header("Prefabs")]
    [SerializeField] private AmmoPickup ammoPrefab;
    [SerializeField] private Checkpoint checkpointPrefab;
    [SerializeField] private GameObject spawnerPrefab;   // только EnemySpawner (префаб-ассет!)
    [SerializeField] private GameObject triggerPrefab;   // только SpawnTrigger + коллайдер (префаб-ассет!)
    [SerializeField] private GameObject[] enemyPrefabs;   // реестр типов врагов, индекс = тип

    [SerializeField, TextArea(10, 30)]
    private string levelJson;

    // ---------- СОХРАНЕНИЕ ----------
    [ContextMenu(nameof(SaveCurrentLevel))]
    public void SaveCurrentLevel()
    {
        var saveData = LoadSaveData();

        var level = saveData.Levels.FirstOrDefault(x => x.DifficultType == CurrentDifficult);
        if (level == null)
        {
            level = new DifficultLevelData { DifficultType = CurrentDifficult };
            saveData.Levels.Add(level);
        }

        level.AmmoPositions = FindAll<AmmoPickup>().Select(a => a.transform.position).ToList();
        level.Checkpoints = FindAll<Checkpoint>().Select(c =>
                {
                    var box = c.GetComponent<BoxCollider2D>();
                    return new CheckpointEntry
                    {
                        Position = c.transform.position,
                        RotationZ = c.transform.eulerAngles.z,
                        Scale = c.transform.localScale,
                        ColliderSize = box != null ? box.size : Vector2.one,
                        ColliderOffset = box != null ? box.offset : Vector2.zero
                    };
                }).ToList();

        // Триггеры: плоский список позиций + словарь для индексов
        var triggers = FindAll<SpawnTrigger>().ToList();
        level.Triggers = triggers.Select(t =>
        {
            var box = t.GetComponent<BoxCollider2D>();
            return new TriggerEntry
            {
                Position = t.transform.position,
                RotationZ = t.transform.eulerAngles.z,
                Scale = t.transform.localScale,
                ColliderSize = box != null ? box.size : Vector2.one,
                ColliderOffset = box != null ? box.offset : Vector2.zero
            };
        }).ToList();

        // Спавнеры: позиция + ссылка на триггер по индексу + настройки
        level.Spawners = FindAll<EnemySpawner>().Select(s =>
        {
            int triggerIndex = s.Trigger != null ? triggers.IndexOf(s.Trigger) : -1;

            if (s.Trigger != null && triggerIndex < 0)
                Debug.LogWarning($"Спавнер {s.name}: его триггер не найден в общем списке.");
            if (Array.IndexOf(enemyPrefabs, s.EnemyPrefab) < 0)
                Debug.LogWarning($"Спавнер {s.name}: префаб врага не в массиве Enemy Prefabs.");

            return new SpawnerEntry
            {
                SpawnerPosition = s.transform.position,
                TriggerIndex = triggerIndex,
                EnemyPrefabIndex = Array.IndexOf(enemyPrefabs, s.EnemyPrefab),
                EnemyCount = s.EnemyCount,
                SpawnInterval = s.SpawnInterval,
                SpawnRadius = s.SpawnRadius
            };
        }).ToList();

        levelJson = JsonUtility.ToJson(saveData, true);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"Saved {CurrentDifficult}: триггеров {level.Triggers.Count}, спавнеров {level.Spawners.Count}.");
    }

    // ---------- ЗАГРУЗКА ----------
    [ContextMenu(nameof(LoadCurrentLevel))]
    public void LoadCurrentLevel()
    {
        if (spawnerPrefab == null || triggerPrefab == null)
        {
            Debug.LogError("Spawner Prefab или Trigger Prefab не назначен — загрузка отменена.");
            return;
        }

        ClearCurrentLevel();

        var saveData = LoadSaveData();
        var level = saveData.Levels.FirstOrDefault(x => x.DifficultType == CurrentDifficult);
        if (level == null)
        {
            Debug.LogWarning($"Нет данных для сложности {CurrentDifficult}.");
            return;
        }

        // Патроны
        var ammoParent = FindFirstObjectByType<AmmoParent>();
        foreach (var pos in level.AmmoPositions)
        {
            var ammo = Instantiate(ammoPrefab, pos, Quaternion.identity);
            if (ammoParent != null) ammo.transform.SetParent(ammoParent.transform);
        }

        // Чекпоинты
        var checkpointRoot = GetOrCreateCheckpointRoot();
        foreach (var data in level.Checkpoints)
        {
            var obj = Instantiate(checkpointPrefab, data.Position,
                Quaternion.Euler(0f, 0f, data.RotationZ), checkpointRoot.transform);
            obj.transform.localScale = data.Scale;

            var box = obj.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = data.ColliderSize;
                box.offset = data.ColliderOffset;
            }
        }

        // Корень Spawners (создаём, если нет)
        var root = GetOrCreateSpawnerRoot();

        // Триггеры — плоско под Spawners, запоминаем по индексу
        var triggers = new SpawnTrigger[level.Triggers.Count];
        for (int i = 0; i < level.Triggers.Count; i++)
        {
            var data = level.Triggers[i];

            var obj = Instantiate(triggerPrefab, data.Position,
                Quaternion.Euler(0f, 0f, data.RotationZ), root.transform);
            obj.name = $"SpawnTrigger ({i})";
            obj.transform.localScale = data.Scale;

            var box = obj.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = data.ColliderSize;
                box.offset = data.ColliderOffset;
            }

            triggers[i] = obj.GetComponent<SpawnTrigger>();
        }
        // Спавнеры — плоско под Spawners, каждый привязываем к своему триггеру
        for (int i = 0; i < level.Spawners.Count; i++)
        {
            var data = level.Spawners[i];

            var obj = Instantiate(spawnerPrefab, data.SpawnerPosition, Quaternion.identity, root.transform);
            obj.name = $"Spawner ({i})";

            var spawner = obj.GetComponent<EnemySpawner>();
            if (spawner == null)
            {
                Debug.LogError("На Spawner Prefab нет компонента EnemySpawner.");
                continue;
            }

            SpawnTrigger trigger = (data.TriggerIndex >= 0 && data.TriggerIndex < triggers.Length)
                ? triggers[data.TriggerIndex]
                : null;
            spawner.SetTrigger(trigger);

            var enemyPrefab = (data.EnemyPrefabIndex >= 0 && data.EnemyPrefabIndex < enemyPrefabs.Length)
                ? enemyPrefabs[data.EnemyPrefabIndex]
                : null;
            spawner.Configure(enemyPrefab, data.EnemyCount, data.SpawnInterval, data.SpawnRadius);
        }

        Debug.Log($"Loaded {CurrentDifficult}: триггеров {triggers.Length}, спавнеров {level.Spawners.Count}.");
    }

    // ---------- ВСПОМОГАТЕЛЬНОЕ ----------
    private void ClearCurrentLevel()
    {
        DestroyAll<AmmoPickup>();
        DestroyAll<Checkpoint>();
        DestroyAll<EnemySpawner>();
        DestroyAll<SpawnTrigger>();
    }

    private SpawnerParent GetOrCreateSpawnerRoot()
    {
        var root = FindFirstObjectByType<SpawnerParent>(FindObjectsInactive.Include);
        if (root == null)
        {
            var go = new GameObject("Spawners");
            root = go.AddComponent<SpawnerParent>();
            Debug.Log("Создан корень Spawners (SpawnerParent отсутствовал).");
        }
        return root;
    }

    private CheckpointParent GetOrCreateCheckpointRoot()
    {
        var root = FindFirstObjectByType<CheckpointParent>(FindObjectsInactive.Include);
        if (root == null)
        {
            var go = new GameObject("Checkpoints");
            root = go.AddComponent<CheckpointParent>();
            Debug.Log("Создан корень Checkpoints (CheckpointParent отсутствовал).");
        }
        return root;
    }

    private static T[] FindAll<T>() where T : Component =>
        FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

    private static void DestroyAll<T>() where T : Component
    {
        foreach (var obj in FindAll<T>())
            DestroyImmediate(obj.gameObject);
    }

    private SaveData LoadSaveData()
    {
        if (string.IsNullOrWhiteSpace(levelJson))
            return new SaveData();

        return JsonUtility.FromJson<SaveData>(levelJson) ?? new SaveData();
    }

    // ---------- ДАННЫЕ ----------
    [Serializable]
    public class SaveData
    {
        public List<DifficultLevelData> Levels = new();
    }

    [Serializable]
    public class DifficultLevelData
    {
        public Difficult.DifficultType DifficultType;
        public List<Vector3> AmmoPositions = new();
        public List<CheckpointEntry> Checkpoints = new();
        public List<TriggerEntry> Triggers = new();
        public List<SpawnerEntry> Spawners = new();
    }

    [Serializable]
    public class SpawnerEntry
    {
        public Vector3 SpawnerPosition;
        public int TriggerIndex;        // индекс в TriggerPositions, -1 если без триггера
        public int EnemyPrefabIndex;
        public int EnemyCount;
        public float SpawnInterval;
        public float SpawnRadius;
    }
    [Serializable]
    public class TriggerEntry
    {
        public Vector3 Position;
        public Vector3 Scale = Vector3.one;
        public float RotationZ;
        public Vector2 ColliderSize = Vector2.one;
        public Vector2 ColliderOffset;
    }

    [Serializable]
    public class CheckpointEntry
    {
        public Vector3 Position;
        public Vector3 Scale = Vector3.one;
        public float RotationZ;
        public Vector2 ColliderSize = Vector2.one;
        public Vector2 ColliderOffset;
    }
}
