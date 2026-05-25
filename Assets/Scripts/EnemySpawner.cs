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

    public SpawnTrigger Trigger => spawnTrigger;
    public GameObject EnemyPrefab => enemyPrefab;
    public int EnemyCount => enemyCount;
    public float SpawnInterval => spawnInterval;
    public float SpawnRadius => spawnRadius;

    public void SetTrigger(SpawnTrigger trigger)
    {
        spawnTrigger = trigger;
    }

    public void Configure(GameObject prefab, int count, float interval, float radius)
    {
        enemyPrefab = prefab;
        enemyCount = count;
        spawnInterval = interval;
        spawnRadius = radius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }

    private void Awake()
    {
        if (spawnTrigger != null)
        {
            spawnTrigger.Triggered += OnTriggered;
        }
    }

    private void OnDisable()
    {
        if (spawnTrigger != null)
        {
            spawnTrigger.Triggered -= OnTriggered;
        }
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
        var profile = DifficultyState.Current;
        int count = enemyCount;
        float interval = spawnInterval;

        if (profile != null)
        {
            count = Mathf.Max(0, Mathf.RoundToInt(enemyCount * profile.enemyCountMultiplier));
            interval = spawnInterval * profile.spawnIntervalMultiplier;
        }
        
        for (int i = 0; i < count; i++)
        {
            // Небольшой случайный разброс, чтобы враги не спавнились в одной точке
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Пауза перед следующим спавном
            yield return new WaitForSeconds(interval);
        }
    }

    public void Rollback()
    {
        StopAllCoroutines();
        hasTriggered = false;
    }
}
