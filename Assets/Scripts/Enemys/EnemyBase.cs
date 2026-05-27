using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float loseInterestRange = 9f;

    [Header("Урон")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private bool useContactDamage = true;

    [Header("Здоровье")]
    [SerializeField] protected int hitPoints = 1;

    protected Rigidbody2D rb;
    protected Transform player;
    private float damageTimer;
    private bool isChasing = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        var profile = DifficultyState.Current;
        if (profile != null)
        {
            moveSpeed = profile.enemyMoveSpeed;
            damage = profile.enemyDamage;
            damageInterval = profile.enemyDamageInterval;
        }
    }

    private void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // Игрок мёртв — замираем, не преследуем
        if (!PlayerHealth.IsPlayerAlive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(rb.position, player.position);

        if (!isChasing && distance <= detectionRange)
            isChasing = true;
        else if (isChasing && distance >= loseInterestRange)
            isChasing = false;

        if (isChasing)
            rb.linearVelocity = GetMoveDirection() * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    // Полиморфная точка: каждый наследник задаёт направление по-своему
    protected abstract Vector2 GetMoveDirection();

    // Хелпер для наследников: чистое направление к игроку
    protected Vector2 DirectionToPlayer()
    {
        return ((Vector2)player.position - rb.position).normalized;
    }

    public virtual bool TakeHit(int amount = 1)
    {
        hitPoints -= amount;
        return hitPoints <= 0;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!useContactDamage) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        if (damageTimer > 0f) return;

        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            damageTimer = damageInterval;
        }
    }
}
