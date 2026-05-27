using System;
using System.Collections.Generic;
using UnityEngine;

// Профиль ограничения для одного условия. Геометрия уровня одинакова во всех трёх —
// меняются только эти числа. Три блока = три рычага напряжения.
[Serializable]
public class DifficultyProfile
{
    [Header("Player")]
    public int playerStartAmmo = 12;

    [Header("Global settings")]
    public float enemyMoveSpeed = 2.8f;
    public int enemyDamage = 1;
    public float enemyDamageInterval = 1f;

    [Header("Manual configuration of level objects")]
    public List<SpawnerBalanceConfig> spawners = new();
    public List<SpawnTriggerBalanceConfig> spawnTriggers = new();
    public List<AmmoPickupBalanceConfig> ammoPickups = new();
    public List<CheckpointBalanceConfig> checkpoints = new();

    public SpawnerBalanceConfig FindSpawner(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return spawners.Find(x => x.id == id);
    }

    public AmmoPickupBalanceConfig FindAmmoPickup(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return ammoPickups.Find(x => x.id == id);
    }

    public CheckpointBalanceConfig FindCheckpoint(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return checkpoints.Find(x => x.id == id);
    }

    public SpawnTriggerBalanceConfig FindSpawnTrigger(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return spawnTriggers.Find(x => x.id == id);
    }
}

[Serializable]
public class SpawnerBalanceConfig
{
    public string id;
    public bool enabled = true;
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public float spawnInterval = 1f;
    public float spawnRadius = 1f;
}

[Serializable]
public class AmmoPickupBalanceConfig
{
    public string id;
    public bool enabled = true;
    public int ammoAmount = 5;
}

[Serializable]
public class CheckpointBalanceConfig
{
    public string id;
    public bool enabled = true;
}

[Serializable]
public class SpawnTriggerBalanceConfig
{
    public string id;
    public bool enabled = true;
}

// Текущий профиль, который читают враги в момент появления.
public static class DifficultyState
{
    public static DifficultyProfile Current;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Current = null;
}
