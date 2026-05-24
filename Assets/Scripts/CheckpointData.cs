using UnityEngine;

// Живёт только во время сессии, на диск ничего не пишется
public static class CheckpointData
{
    public static bool HasCheckpoint = false;
    public static Vector2 Position;
    public static int SavedAmmo;
    public static int SavedHealth;
}
