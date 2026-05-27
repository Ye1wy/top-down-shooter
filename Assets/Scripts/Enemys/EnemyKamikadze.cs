using UnityEngine;

public class EnemyKamikaze : EnemyBase
{
    [Header("Синусоидальное движение")]
    [SerializeField] private float waveAmplitude = 0.6f;
    [SerializeField] private float waveFrequency = 5f;
    [SerializeField] private bool randomizeWavePhase = true;

    [Header("Взрыв")]
    [SerializeField] private float explosionRadius = 1.2f;
    [SerializeField] private int explosionDamage = 1;
    [SerializeField] private GameObject explosionVFXPrefab;

    private float wavePhase;
    private bool exploded;

    protected override void Start()
    {
        base.Start();

        if (randomizeWavePhase)
            wavePhase = Random.Range(0f, Mathf.PI * 2f);

        var profile = DifficultyState.Current;
        if (profile != null)
            explosionDamage = profile.enemyDamage;
    }

    protected override Vector2 GetMoveDirection()
    {
        Vector2 forward = DirectionToPlayer();

        if (forward.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        Vector2 side = new Vector2(-forward.y, forward.x);

        float wave = Mathf.Sin(Time.time * waveFrequency + wavePhase) * waveAmplitude;

        return (forward + side * wave).normalized;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (exploded) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        rb.linearVelocity = Vector2.zero;

        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.Euler(90, 0, 0));

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            if (hit.TryGetComponent<PlayerHealth>(out PlayerHealth health))
                health.TakeDamage(explosionDamage);

            break;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
