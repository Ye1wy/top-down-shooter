using System.Collections;
using UnityEngine;

public class EnemyShooter : EnemyBase
{
    [Header("Стрельба")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 6f;
    [SerializeField] private float shootInterval = 1.5f;
    [SerializeField] private float shootRange = 7f;     // не стреляет, если игрок дальше
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("Дистанция боя")]
    [SerializeField] private float preferredRange = 5f;     // комфортная дистанция
    [SerializeField] private float rangeTolerance = 0.5f;   // ширина "комфортной зоны"

    protected override void Start()
    {
        base.Start();
        StartCoroutine(ShootRoutine());
    }

    protected override Vector2 GetMoveDirection()
    {
        if (player == null) return Vector2.zero;

        Vector2 toPlayer = DirectionToPlayer();
        float distance = Vector2.Distance(rb.position, player.position);

        // Слишком близко — пятимся, держим дистанцию для прицельной стрельбы
        if (distance < preferredRange - rangeTolerance)
            return -toPlayer;

        // Слишком далеко — подходим в зону стрельбы
        if (distance > preferredRange + rangeTolerance)
            return toPlayer;

        // В зоне комфорта — стоим и стреляем
        return Vector2.zero;
    }

    private IEnumerator ShootRoutine()
    {
        // Случайный сдвиг фазы — группа стрелков не палит синхронно (иначе пик урона = чистая лотерея)
        yield return new WaitForSeconds(Random.Range(0f, shootInterval));

        while (true)
        {
            yield return new WaitForSeconds(shootInterval);

            // Игрок мёртв или сцена в паузе (timeScale=0 уже останавливает корутину) — пропускаем
            if (player == null || !PlayerHealth.IsPlayerAlive) continue;

            float distance = Vector2.Distance(rb.position, player.position);
            if (distance > shootRange) continue;

            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"{name}: bulletPrefab не назначен", this);
            return;
        }

        Vector2 direction = DirectionToPlayer();
        Vector3 spawnPos = transform.position + (Vector3)direction * spawnOffset;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        if (bullet.TryGetComponent<Rigidbody2D>(out var bulletRb))
            bulletRb.linearVelocity = direction * bulletSpeed;

        // Чтобы снаряд не самоуничтожился о коллайдер стрелка при вылете
        Collider2D myCol = GetComponent<Collider2D>();
        Collider2D bulletCol = bullet.GetComponent<Collider2D>();
        if (myCol != null && bulletCol != null)
            Physics2D.IgnoreCollision(bulletCol, myCol);
    }
}
