using UnityEngine;
using System;

public class SpawnTrigger : MonoBehaviour
{
    [Header("Balace Id")]
    [SerializeField] private string balanceId;

    public string BalanceId => string.IsNullOrWhiteSpace(balanceId) ? gameObject.name : balanceId;

    public event Action Triggered;

    public void ApplyBalance(SpawnTriggerBalanceConfig config)
    {
        if (config == null) return;

        if (gameObject.activeSelf != config.enabled)
            gameObject.SetActive(config.enabled);
    }

    public SpawnTriggerBalanceConfig CaptureBalance()
    {
        return new SpawnTriggerBalanceConfig
        {
            id = BalanceId,
            enabled = gameObject.activeSelf,
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

        Triggered?.Invoke();
    }
}
