using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private ParticleSystem DeathVFXPrefab;


    private int currentHealth;
    private Vector3 startPosition;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    // Публичный — чтобы враг мог нанести урон извне
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Получен урон! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Respawn();
        }
    }

    private void Die()
    {
        Debug.Log("Игрок погиб");
        Instantiate(DeathVFXPrefab, transform.position, Quaternion.Euler(90, 0, 0));
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Respawn()
    {
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
        Debug.Log("Здоровье восстановлено! HP: " + currentHealth);
    }
}
