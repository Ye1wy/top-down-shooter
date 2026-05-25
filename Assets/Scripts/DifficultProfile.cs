using System;
using UnityEngine;
 
// Профиль ограничения для одного условия. Геометрия уровня одинакова во всех трёх —
// меняются только эти числа. Три блока = три рычага напряжения.
[Serializable]
public class DifficultyProfile
{
    [Header("Ограничение ресурсов")]
    public int playerStartAmmo = 12;     // стартовый боезапас
    public int ammoPickupAmount = 5;     // сколько даёт один пикап
 
    [Header("Сильный противник")]
    public float enemyMoveSpeed = 2.8f;
    public int enemyDamage = 1;
    public float enemyDamageInterval = 1f;     // пауза между ударами врага (меньше = больнее)
    public float enemyCountMultiplier = 1f;    // множитель к авторской численности каждого спавнера
    public float spawnIntervalMultiplier = 1f; // множитель к паузе спавна (<1 = чаще = сложнее)
 
    [Header("Угроза потери прогресса")]
    public int activeCheckpointCount = -1;     // сколько чекпоинтов активно; -1 = все
}
 
// Текущий профиль, который читают враги и спавнеры в момент появления.
public static class DifficultyState
{
    public static DifficultyProfile Current;
 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() => Current = null;
}
