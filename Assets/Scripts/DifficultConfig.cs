using System;
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
    [Header("Component condition")]
    public Difficult.DifficultType difficult = Difficult.DifficultType.Normal;

    [Header("Panel settings selected difficult")]
    public DifficultyProfile profile = new DifficultyProfile();

    [SerializeField, HideInInspector] private Difficult.DifficultType lastDifficulty = Difficult.DifficultType.Normal;
    [SerializeField, HideInInspector] private bool initialized;

    [SerializeField, HideInInspector] private DifficultyProfileSlot easy = new DifficultyProfileSlot();
    [SerializeField, HideInInspector] private DifficultyProfileSlot normal = new DifficultyProfileSlot();
    [SerializeField, HideInInspector] private DifficultyProfileSlot hard = new DifficultyProfileSlot();

    public DifficultyProfile CurrentProfile => profile;
    public bool CurrentDifficultyHasSavedLevelBalance => HasSavedLevelBalance(difficult);

    private void OnEnable()
    {
        EnsureInitialized();
        LoadSelectedDifficultyToPanel(saveAsset: false);
    }

    private void OnValidate()
    {
        EnsureInitialized();

        if (difficult == lastDifficulty)
            return;

        // При смене difficult не меняем API и не прячем profile.
        // Просто накатываем в публичную панель profile сохранённые значения выбранной сложности.
        // Если сохранения ещё нет — накатываем дефолт этой сложности.
        LoadSelectedDifficultyToPanel(saveAsset: false);
        lastDifficulty = difficult;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public DifficultyProfile GetProfile(Difficult.DifficultType type)
    {
        EnsureInitialized();

        var slot = GetSlot(type);
        if (!slot.hasSavedLevelBalance)
            slot.profile.CopyFrom(DifficultyProfiles.Create(type));

        return slot.profile;
    }

    public void SetProfile(Difficult.DifficultType type, DifficultyProfile source, bool markAsSaved = true)
    {
        EnsureInitialized();

        var slot = GetSlot(type);
        slot.profile.CopyFrom(source != null ? source : DifficultyProfiles.Create(type));
        slot.hasSavedLevelBalance = markAsSaved;

        if (type == difficult)
            profile.CopyFrom(slot.profile);

        SaveAssetNow();
    }

    public bool HasSavedLevelBalance(Difficult.DifficultType type)
    {
        EnsureInitialized();
        return GetSlot(type).hasSavedLevelBalance;
    }

    public void SelectDifficulty(Difficult.DifficultType type)
    {
        difficult = type;
        LoadSelectedDifficultyToPanel(saveAsset: true);
        lastDifficulty = difficult;
    }

    public void LoadSelectedDifficultyToPanel(bool saveAsset = true)
    {
        EnsureInitialized();

        var slot = GetSlot(difficult);
        DifficultyProfile source = slot.hasSavedLevelBalance
            ? slot.profile
            : DifficultyProfiles.Create(difficult);

        profile.CopyFrom(source);

        if (saveAsset)
            SaveAssetNow();
    }


    public void SavePanelToSelectedDifficulty()
    {
        EnsureInitialized();

        var slot = GetSlot(difficult);
        slot.profile.CopyFrom(profile);
        slot.hasSavedLevelBalance = true;

        SaveAssetNow();
        Debug.Log($"Saved panel balance for {difficult}");
    }

    public void ResetSelectedDifficultyToDefault()
    {
        EnsureInitialized();

        profile.CopyFrom(DifficultyProfiles.Create(difficult));

        var slot = GetSlot(difficult);
        slot.profile.CopyFrom(profile);
        slot.hasSavedLevelBalance = false;

        SaveAssetNow();
        Debug.Log($"Reset {difficult} balance to defaults");
    }

    public void SaveCurrentLevel()
    {
        EnsureInitialized();

        CaptureCurrentSceneToPanel();
        SavePanelToSelectedDifficulty();

        Debug.Log($"Saved current scene balance for {difficult}");
    }

    public void LoadCurrentLevel()
    {
        EnsureInitialized();

        LoadSelectedDifficultyToPanel(saveAsset: false);
        ApplyPanelToCurrentScene();

        SaveAssetNow();
        Debug.Log($"Loaded {difficult} balance to current scene");
    }

    public void CaptureCurrentSceneToPanel()
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
    }

    public void ApplyPanelToCurrentScene()
    {
        ApplyProfileToCurrentScene(profile);
    }

    public void ApplyProfileToCurrentScene(DifficultyProfile source)
    {
        if (source == null)
            return;

        foreach (var spawner in FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = source.FindSpawner(spawner.BalanceId);
            if (config != null)
                spawner.ApplyBalance(config);
        }

        foreach (var ammo in FindObjectsByType<AmmoPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = source.FindAmmoPickup(ammo.BalanceId);
            if (config != null)
                ammo.ApplyBalance(config);
        }

        foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = source.FindCheckpoint(checkpoint.BalanceId);
            if (config != null)
                checkpoint.ApplyBalance(config);
        }
    }

    private void EnsureInitialized()
    {
        if (easy == null) easy = new DifficultyProfileSlot();
        if (normal == null) normal = new DifficultyProfileSlot();
        if (hard == null) hard = new DifficultyProfileSlot();
        if (profile == null) profile = new DifficultyProfile();

        if (initialized)
            return;

        easy.profile.CopyFrom(DifficultyProfiles.Create(Difficult.DifficultType.Easy));
        normal.profile.CopyFrom(DifficultyProfiles.Create(Difficult.DifficultType.Normal));
        hard.profile.CopyFrom(DifficultyProfiles.Create(Difficult.DifficultType.Hard));

        profile.CopyFrom(DifficultyProfiles.Create(difficult));
        lastDifficulty = difficult;
        initialized = true;
    }

    private bool IsProfileEmpty(DifficultyProfile profile)
    {
        return profile.spawners.Count == 0
            && profile.ammoPickups.Count == 0
            && profile.checkpoints.Count == 0
            && profile.playerStartAmmo == 12
            && Mathf.Approximately(profile.enemyMoveSpeed, 2.8f)
            && profile.enemyDamage == 1
            && Mathf.Approximately(profile.enemyDamageInterval, 0.9f);
    }

    private DifficultyProfileSlot GetSlot(Difficult.DifficultType type)
    {
        return type switch
        {
            Difficult.DifficultType.Easy => easy,
            Difficult.DifficultType.Normal => normal,
            Difficult.DifficultType.Hard => hard,
            _ => normal
        };
    }

    private void SaveAssetNow()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

    [Serializable]
    private class DifficultyProfileSlot
    {
        public bool hasSavedLevelBalance;
        public DifficultyProfile profile = new DifficultyProfile();
    }
}
