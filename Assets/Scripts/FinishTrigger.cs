using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    [SerializeField] private GameFlowManager gameFlow;
    private bool hasFinished = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerShooting>(out var _))
            return;

        if (hasFinished) return;
        hasFinished = true;

        if (gameFlow == null)
            gameFlow = FindFirstObjectByType<GameFlowManager>();
        
        if (gameFlow != null)
            gameFlow.Finish();
    }
}
