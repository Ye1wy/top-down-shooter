using UnityEngine;

public class TelemetryProgressMarker : MonoBehaviour
{
    [SerializeField] private float progressPercent = 25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Игрока определяем так же, как чекпоинты — по наличию PlayerShooting
        if (!other.TryGetComponent<PlayerShooting>(out var _))
            return;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.RegisterProgress(progressPercent);
    }
}
