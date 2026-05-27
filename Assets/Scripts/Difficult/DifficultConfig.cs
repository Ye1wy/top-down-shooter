using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


// Asset-конфиг одной сложности.
// Создай три asset-а: EasyBalance, NormalBalance, HardBalance.
// Настраивай сцену руками, затем нажимай Save Current Level в инспекторе конфига.
[CreateAssetMenu(menuName = "Game/Balance/Difficulty Config")]
public class DifficultConfig : ScriptableObject
{
    [Header("Сложность")]
    public Difficult.DifficultType difficult = Difficult.DifficultType.Normal;

    [Header("Сохраненный баланс")]
    public DifficultyProfile profile = new DifficultyProfile();

    public DifficultyProfile Profile => profile;

    public void SaveSceneToAsset()
    {
        profile.spawners.Clear();
        foreach (var spawner in FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            profile.spawners.Add(spawner.CaptureBalance());

        profile.ammoPickups.Clear();
        foreach (var ammo in FindObjectsByType<AmmoPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            profile.ammoPickups.Add(ammo.CaptureBalance());

        profile.checkpoints.Clear();
        foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            profile.checkpoints.Add(checkpoint.CaptureBalance());

        profile.spawnTriggers.Clear();
        foreach (var trigger in FindObjectsByType<SpawnTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            profile.spawnTriggers.Add(trigger.CaptureBalance());

        MarkAssetDirty();
        Debug.Log($"[{name}] Снимок сцены сохранён для {difficult}.");
    }

    // --- Применить ассет к сцене ----------------------------------------------
    public void ApplyToScene()
    {
        foreach (var spawner in FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = profile.FindSpawner(spawner.BalanceId);
            if (config != null)
            {
                spawner.ApplyBalance(config);
                MarkSceneObjectDirty(spawner);
            }
        }

        foreach (var ammo in FindObjectsByType<AmmoPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = profile.FindAmmoPickup(ammo.BalanceId);
            if (config != null)
            {
                ammo.ApplyBalance(config);
                MarkSceneObjectDirty(ammo);
            }
        }

        foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = profile.FindCheckpoint(checkpoint.BalanceId);
            if (config != null)
            {
                checkpoint.ApplyBalance(config);
                MarkSceneObjectDirty(checkpoint);
            }
        }

        foreach (var trigger in FindObjectsByType<SpawnTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = profile.FindSpawnTrigger(trigger.BalanceId);
            if (config != null)
            {
                trigger.ApplyBalance(config);
                MarkSceneObjectDirty(trigger);
            }
        }

        MarkSceneDirty();
        Debug.Log($"[{name}] Баланс {difficult} применён к сцене.");
    }

    // --- Editor helpers --------------------------------------------------------
    private void MarkAssetDirty()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

    private static void MarkSceneObjectDirty(Object obj)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(obj);
#endif
    }

    private static void MarkSceneDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
    }
}
