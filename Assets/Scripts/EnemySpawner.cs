using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour, IConsumable
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 5;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private SpawnTrigger spawnTrigger;

    private bool hasTriggered = false;

    private void Awake()
    {
        spawnTrigger.Triggered += OnTriggered;
    }

    private void OnDisable()
    {
        spawnTrigger.Triggered -= OnTriggered;
    }

    private void OnTriggered()
    {
        if (hasTriggered) return;

        hasTriggered = true;
        WorldState.RegisterConsumed(this);
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // Небольшой случайный разброс, чтобы враги не спавнились в одной точке
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Пауза перед следующим спавном
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void Rollback()
    {
        StopAllCoroutines();
        hasTriggered = false;
    }
}
