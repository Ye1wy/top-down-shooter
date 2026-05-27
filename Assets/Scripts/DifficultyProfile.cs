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

    public void CopyFrom(DifficultyProfile other)
    {
        if (other == null) return;

        playerStartAmmo = other.playerStartAmmo;
        enemyMoveSpeed = other.enemyMoveSpeed;
        enemyDamage = other.enemyDamage;
        enemyDamageInterval = other.enemyDamageInterval;

        spawners = new List<SpawnerBalanceConfig>();
        foreach (var x in other.spawners)
            spawners.Add(x.Clone());

        ammoPickups = new List<AmmoPickupBalanceConfig>();
        foreach (var x in other.ammoPickups)
            ammoPickups.Add(x.Clone());

        checkpoints = new List<CheckpointBalanceConfig>();
        foreach (var x in other.checkpoints)
            checkpoints.Add(x.Clone());
    }

    public DifficultyProfile Clone()
    {
        var copy = new DifficultyProfile();
        copy.CopyFrom(this);
        return copy;
    }
}

[Serializable]
public class SpawnerBalanceConfig
{
    public string id;
    public GameObject enemyPrefab;
    public int enemyCount = 5;
    public float spawnInterval = 1f;
    public float spawnRadius = 1f;

    public SpawnerBalanceConfig Clone()
    {
        return new SpawnerBalanceConfig
        {
            id = id,
            enemyPrefab = enemyPrefab,
            enemyCount = enemyCount,
            spawnInterval = spawnInterval,
            spawnRadius = spawnRadius
        };
    }
}

[Serializable]
public class AmmoPickupBalanceConfig
{
    public string id;
    public int ammoAmount = 5;

    public AmmoPickupBalanceConfig Clone()
    {
        return new AmmoPickupBalanceConfig
        {
            id = id,
            ammoAmount = ammoAmount
        };
    }
}

[Serializable]
public class CheckpointBalanceConfig
{
    public string id;
    public bool enabled = true;

    public CheckpointBalanceConfig Clone()
    {
        return new CheckpointBalanceConfig
        {
            id = id,
            enabled = enabled
        };
    }
}

// Текущий профиль, который читают враги в момент появления.
public static class DifficultyState
{
    public static DifficultyProfile Current;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Current = null;
}
