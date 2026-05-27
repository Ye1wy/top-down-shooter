using UnityEngine;

// Asset-конфиг одной сложности.
// Создай три asset-а: EasyBalance, NormalBalance, HardBalance.
// Настраивай сцену руками, затем нажимай Save Current Level в инспекторе конфига.
[CreateAssetMenu(menuName = "Game/Balance/Difficulty Config")]
public class DifficultConfig : ScriptableObject
{
    [Header("Component condition")]
    public Difficult.DifficultType difficult = Difficult.DifficultType.Normal;

    [Header("Balance settings")]
    public DifficultyProfile profile = new DifficultyProfile();

    public void SaveCurrentLevel()
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

        Debug.Log($"Saved balance config: {difficult}", this);
    }

    public void LoadCurrentLevel()
    {
        foreach (var spawner in FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = profile.FindSpawner(spawner.BalanceId);
            if (config != null)
                spawner.ApplyBalance(config);
        }

        foreach (var ammo in FindObjectsByType<AmmoPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = profile.FindAmmoPickup(ammo.BalanceId);
            if (config != null)
                ammo.ApplyBalance(config);
        }

        foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var config = profile.FindCheckpoint(checkpoint.BalanceId);
            if (config != null)
                checkpoint.ApplyBalance(config);
        }

        Debug.Log($"Loaded balance config: {difficult}", this);
    }
}
