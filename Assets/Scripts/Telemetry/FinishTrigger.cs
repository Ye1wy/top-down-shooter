using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private bool hasFinished = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerShooting>(out var _))
            return;

        if (hasFinished) return;
        hasFinished = true;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.RegisterCompleted();
    }
}
