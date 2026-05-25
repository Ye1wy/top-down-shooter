using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private ParticleSystem DeathVFXPrefab;
    [SerializeField] private float deathDelay = 1.5f;
    [SerializeField] private TextMeshProUGUI healthText;

    private int currentHealth;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        UpdateHealthUI();
    }

    // Публичный — чтобы враг мог нанести урон извне
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.RegisterDamageTaken(amount, currentHealth);

        UpdateHealthUI();
        Debug.Log("Получен урон! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    private void Die()
    {
        Debug.Log("Игрок погиб");
        Instantiate(DeathVFXPrefab, transform.position, Quaternion.Euler(90, 0, 0));
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;
        IsPlayerAlive = false;

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.RegisterDeath(transform.position);

        Debug.Log("Respawn: Игрок погиб");

        if (DeathVFXPrefab != null)
        {
            Instantiate(DeathVFXPrefab, transform.position, Quaternion.Euler(90, 0, 0));
        }

        rb.linearVelocity = Vector2.zero;
        SetPlayerActive(false);

        yield return new WaitForSeconds(deathDelay);

        Respawn();

        SetPlayerActive(true);
        isDead = false;
        IsPlayerAlive = true;
    }

    private void Respawn()
    {
        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.RegisterRespawn();

        Debug.Log("Игрок погиб, возрождение");

        // Позиция: чекпоинт или старт
        Vector3 respawnPos = CheckpointData.HasCheckpoint
            ? new Vector3(CheckpointData.Position.x, CheckpointData.Position.y, transform.position.z)
            : startPosition;

        rb.linearVelocity = Vector2.zero;
        rb.position = respawnPos;
        transform.position = respawnPos;

        // Полное здоровье
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Откат мира к снимку: возвращаем пикапы и перезаряжаем спавнеры за чекпоинтом
        WorldState.Rollback();

        // Убираем всех живых врагов
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
            Destroy(enemy);

        // Возвращаем патроны к значению на момент снимка
        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
            shooting.SetAmmo(CheckpointData.SavedAmmo);
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log("Здоровье восстановлено! HP: " + currentHealth);
    }

    private void SetPlayerActive(bool active)
    {
        // Спрайт может быть на дочернем объекте — выключаем все найденные
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
            sprite.enabled = active;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = active;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = active;

        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = active;
    }

    public static bool IsPlayerAlive { get; private set; } = true;

    public int CurrentHealth => currentHealth;

    public int MaxHealth => maxHealth;

    public static void ResetAlive()
    {
        IsPlayerAlive = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetAliveState()
    {
        IsPlayerAlive = true;
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = "Здоровье: " + currentHealth;
    }
}
