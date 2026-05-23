using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool hasActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerShooting>(out var _))
            return;

        if (hasActivated)
            return;

        hasActivated = true;

        CheckpointData.HasCheckpoint = true;
        CheckpointData.Position = other.transform.position;

        WorldState.Commit();

        if (other.TryGetComponent<PlayerShooting>(out var shooting))
            CheckpointData.SavedAmmo = shooting.GetAmmo();

        // Сохраняем позицию игрока в PlayerPrefs
        if (other.TryGetComponent<PlayerHealth>(out var health))
            health.RestoreFullHealth();

        Debug.Log("Чекпоинт сохранён");
    }
}
