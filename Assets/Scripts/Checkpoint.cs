using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Balance ID")]
    [SerializeField] private string balanceId;

    [Header("Checkpoint Settings")]
    [SerializeField] private int checkpointIndex = 0;
    [SerializeField] private float progressPercent = 0f;

    private bool hasActivated = false;

    public int Index => checkpointIndex;
    public string BalanceId => string.IsNullOrWhiteSpace(balanceId) ? gameObject.name : balanceId;

    public void ResetForNewCondition()
    {
        hasActivated = false;
        gameObject.SetActive(true);
    }

    public void ApplyBalance(CheckpointBalanceConfig config)
    {
        if (config == null) return;

        hasActivated = false;
        gameObject.SetActive(config.enabled);
    }

    public CheckpointBalanceConfig CaptureBalance()
    {
        return new CheckpointBalanceConfig
        {
            id = BalanceId,
            enabled = gameObject.activeSelf
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerShooting>(out var _))
            return;

        if (hasActivated)
            return;

        hasActivated = true;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.RegisterCheckpointReached(checkpointIndex, progressPercent);

        CheckpointData.HasCheckpoint = true;
        CheckpointData.Position = other.transform.position;

        WorldState.Commit();
        AudioManager.Instance?.PlaySfx(AudioManager.Sfx.Checkpoint);

        if (other.TryGetComponent<PlayerShooting>(out var shooting))
            CheckpointData.SavedAmmo = shooting.GetAmmo();

        // Сохраняем позицию игрока в PlayerPrefs
        if (other.TryGetComponent<PlayerHealth>(out var health))
            health.RestoreFullHealth();

        Debug.Log("Чекпоинт сохранён");
    }
}
