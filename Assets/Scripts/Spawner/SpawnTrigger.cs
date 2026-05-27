using UnityEngine;
using System;

public class SpawnTrigger : MonoBehaviour
{
    public event Action Triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerShooting>(out var _))
            return;

        Triggered?.Invoke();
    }
}
