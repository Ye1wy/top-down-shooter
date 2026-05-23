using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChase : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageInterval = 1f;

    private Rigidbody2D rb;
    private Transform player;
    private float damageTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Находим игрока по тегу один раз при старте
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        // Отсчитываем паузу между ударами
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // Направление от врага к игроку
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (damageTimer > 0f) return; // ещё на перезарядке удара

        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            damageTimer = damageInterval; // ставим паузу перед следующим ударом
        }
    }
}
