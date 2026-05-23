using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float spawnOffset = 0.6f;
    [SerializeField] private int ammoCount = 10;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Transform playerModel;   // модель игрока, которую будем поворачивать

    private void Start()
    {
        CheckpointData.SavedAmmo = ammoCount;
        UpdateAmmoUI();
    }

    private void Update()
    {
        // Поворачиваем модель игрока в сторону курсора каждый кадр
        RotateToCursor();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (ammoCount > 0)
            {
                Shoot();
                ammoCount--;
                UpdateAmmoUI();
            }
        }
    }

    private void RotateToCursor()
    {
        if (playerModel == null)
            return;

        // Позиция курсора в мировых координатах
        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        // Направление от модели к курсору
        Vector2 direction = ((Vector2)(mouseWorld - playerModel.position)).normalized;

        // Угол в градусах и поворот вокруг оси Z (для 2D)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        playerModel.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Shoot()
    {
        // Позиция курсора в мировых координатах
        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        // Направление от игрока к курсору
        Vector2 direction = ((Vector2)(mouseWorld - transform.position)).normalized;

        // Спавним чуть впереди игрока, чтобы снаряд не задел его самого
        Vector3 spawnPos = transform.position + (Vector3)direction * spawnOffset;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletSpeed;
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = "Патроны: " + ammoCount;
    }

    public void AddAmmo(int amount)
    {
        ammoCount += amount;
        UpdateAmmoUI();
    }

    public int GetAmmo() => ammoCount;

    public void SetAmmo(int amount)
    {
        ammoCount = amount;
        UpdateAmmoUI();
    }
}
