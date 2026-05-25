using UnityEngine;

// Живёт только во время сессии, на диск ничего не пишется
public static class CheckpointData
{
    public static bool HasCheckpoint = false;
    public static Vector2 Position;
    public static int SavedAmmo;
    public static int SavedHealth;

    // Перед запуском нового конфига: между сценами RuntimeInitialize не срабатывает,
    // поэтому сбрасываем явно, иначе следующий конфиг стартует с чужим чекпоинтом.
    public static void Reset()
    {
        HasCheckpoint = false;
        Position = Vector2.zero;
        SavedAmmo = 0;
        SavedHealth = 0;
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        Reset();
    }
}
