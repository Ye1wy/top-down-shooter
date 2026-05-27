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
    }

    public void ApplyBalance(CheckpointBalanceConfig config)
    {
        if (config == null) return;

        if (gameObject.activeSelf != config.enabled)
            gameObject.SetActive(config.enabled);

        hasActivated = false;
    }

    public CheckpointBalanceConfig CaptureBalance()
    {
        return new CheckpointBalanceConfig
        {
            id = BalanceId,
            enabled = gameObject.activeSelf
        };
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(balanceId))
            balanceId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }
#endif

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
