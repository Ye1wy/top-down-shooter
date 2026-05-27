using UnityEngine;

public class AmmoPickup : MonoBehaviour, IConsumable
{
    [Header("Balance Id")]
    [SerializeField] private string balanceId;

    [Header("Pickup Settings")]
    [SerializeField] private int ammoAmount = 5;

    public string BalanceId => string.IsNullOrWhiteSpace(balanceId) ? gameObject.name : balanceId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerShooting>(out var shooting))
            return;

        // Берём скрипт стрельбы у игрока и докидываем патроны
        shooting.AddAmmo(ammoAmount);

        AudioManager.Instance?.PlaySfx(AudioManager.Sfx.AmmoPickup);

        gameObject.SetActive(false);
        WorldState.RegisterConsumed(this);
    }

    public void ApplyBalance(AmmoPickupBalanceConfig config)
    {
        if (config == null) return;

        if (gameObject.activeSelf != config.enabled)
            gameObject.SetActive(config.enabled);

        ammoAmount = Mathf.Max(0, config.ammoAmount);
    }

    public AmmoPickupBalanceConfig CaptureBalance()
    {
        return new AmmoPickupBalanceConfig
        {
            id = BalanceId,
            enabled = gameObject.activeSelf,
            ammoAmount = ammoAmount
        };
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(balanceId))
            balanceId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }
#endif

    public void Rollback()
    {
        gameObject.SetActive(true);
    }
}
