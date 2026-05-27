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

        ammoAmount = Mathf.Max(0, config.ammoAmount);
    }

    public AmmoPickupBalanceConfig CaptureBalance()
    {
        return new AmmoPickupBalanceConfig
        {
            id = BalanceId,
            ammoAmount = ammoAmount
        };
    }

    public void Rollback()
    {
        gameObject.SetActive(true);
    }
}
