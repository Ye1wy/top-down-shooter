using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour, IConsumable
{
    [Header("Balance ID")]
    [SerializeField] private string balanceId;

    [Header("Spawner Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 5;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadius = 1f;
    [SerializeField] private SpawnTrigger spawnTrigger;

    private bool hasTriggered = false;

    public string BalanceId => string.IsNullOrWhiteSpace(balanceId) ? gameObject.name : balanceId;

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
        enemyCount = Mathf.Max(0, count);
        spawnInterval = Mathf.Max(0.01f, interval);
        spawnRadius = Mathf.Max(0f, radius);
    }

    public void ApplyBalance(SpawnerBalanceConfig config)
    {
        if (config == null) return;

        if (gameObject.activeSelf != config.enabled)
            gameObject.SetActive(config.enabled);

        if (config.enemyPrefab != null)
            enemyPrefab = config.enemyPrefab;

        enemyCount = Mathf.Max(0, config.enemyCount);
        spawnInterval = Mathf.Max(0.01f, config.spawnInterval);
        spawnRadius = Mathf.Max(0f, config.spawnRadius);
    }

    public SpawnerBalanceConfig CaptureBalance()
    {
        return new SpawnerBalanceConfig
        {
            id = BalanceId,
            enabled = gameObject.activeSelf,
            enemyPrefab = enemyPrefab,
            enemyCount = enemyCount,
            spawnInterval = spawnInterval,
            spawnRadius = spawnRadius
        };
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(balanceId))
            balanceId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }
#endif

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

    // Подписку держим на OnEnable/OnDisable: спавнеры переключаются SetActive
    // между конфигурациями, а Awake срабатывает один раз — её одной мало.
    private void OnEnable()
    {
        if (spawnTrigger != null)
            spawnTrigger.Triggered += OnTriggered;
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
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"Spawner '{BalanceId}' has no enemy prefab.", this);
            return;
        }

        AudioManager.Instance?.PlaySfx(AudioManager.Sfx.EnemySpawn);

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
