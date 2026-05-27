using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 4f;

    private void Awake()
    {
        // Урон вражеских снарядов идёт по тому же бюджету, что и контактный урон:
        // профиль сложности задаёт enemyDamage один раз для всех источников.
        var profile = DifficultyState.Current;
        if (profile != null)
            damage = profile.enemyDamage;
    }

    private void Start()
    {
        // Страховка от бесконечно живущих снарядов (вылетел в пустоту — самоликвидируется)
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerShooting>(out var _))
        {
            if (collision.gameObject.TryGetComponent<PlayerHealth>(out var health))
                health.TakeDamage(damage);
        }

        // Снаряд исчезает при любом столкновении (стена, игрок, чужой враг)
        Destroy(gameObject);
    }
}
