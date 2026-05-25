using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private ParticleSystem enemyDeathVFXPrefab;

    private void Start()
    {
        // Уничтожаем снаряд через несколько секунд, чтобы они не копились
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (TelemetryManager.Instance != null)
                TelemetryManager.Instance.RegisterShotHit();

            if (enemyDeathVFXPrefab != null)
            {
                Instantiate(enemyDeathVFXPrefab, collision.gameObject.transform.position, Quaternion.Euler(90, 0, 0));
            }

            Destroy(collision.gameObject);
        }

        // Исчезает при попадании во что-либо (стену, врага)
        Destroy(gameObject);
    }
}
