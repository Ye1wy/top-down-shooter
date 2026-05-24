using UnityEngine;

public class AmmoPickup : MonoBehaviour, IConsumable
{
    [SerializeField] private int ammoAmount = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerShooting>(out var shooting))
            return;

        // Берём скрипт стрельбы у игрока и докидываем патроны
        shooting.AddAmmo(ammoAmount);
        gameObject.SetActive(false);
        WorldState.RegisterConsumed(this);
    }

    public void Rollback()
    {
        gameObject.SetActive(true);
    }
}
