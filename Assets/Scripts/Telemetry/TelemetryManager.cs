using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    [Header("Контекст сессии")]
    [SerializeField] private string participantId = "P001";
    [SerializeField] private int orderIndex = 1;
    [SerializeField] private DifficultConfig config;   // condition_id = CurrentDifficult

    [Header("Ссылки")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShooting playerShooting;

    private TelemetrySessionData data;
    private float sessionStartTime;
    private bool hasSaved;
    private float currentProgressPercent;
    private float checkpointProgressPercent;
    private float safePointTime;
    private int lastDeathZone = -1;
    private bool waitingAfterDeath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        data = new TelemetrySessionData
        {
            participant_id = participantId,
            condition_id = config != null ? config.CurrentDifficult.ToString() : "unknown",
            order_index = orderIndex
        };
        sessionStartTime = Time.time;

        safePointTime = sessionStartTime;
    }

    // Единая точка сохранения; пишет файл только один раз за сессию
    public void SaveSession(bool completed, bool quit)
    {
        if (hasSaved) return;

        if (data == null)
        {
            Debug.LogWarning("Telemetry data is null, cannot save session");
            return;
        }

        hasSaved = true;

        data.completed = completed;
        data.quit = quit;
        data.duration_sec = Time.time - sessionStartTime;
        data.ammo_left = playerShooting != null ? playerShooting.GetAmmo() : 0;
        data.accuracy = data.shots_fired > 0
            ? (float)data.shots_hit / data.shots_fired
            : 0f;

        string fileName =
            $"telemetry_{data.participant_id}_{data.condition_id}_{data.order_index}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log($"Телеметрия сохранена: {path}");
    }

    private void OnApplicationQuit()
    {
        if (!hasSaved)
            SaveSession(false, true);   // вышел до финиша → quit
    }

    // Для ручной проверки прямо из инспектора
    [ContextMenu("Save Session (test)")]
    private void SaveSessionTest()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Сначала запусти игру, потом жми Save Session (test).");
            return;
        }

        SaveSession(false, false);
    }

    // ----------------Shooting----------------
    public void RegisterShotFired()
    {
        if (data == null) return;
        data.shots_fired++;
    }

    public void RegisterAmmoEmpty()
    {
        if (data == null) return;
        data.ammo_empty_count++;
    }

    public void RegisterShotHit()
    {
        if (data == null) return;
        data.shots_hit++;
    }

    public void Update()
    {
        if (data == null) return;

        if (playerHealth != null && PlayerHealth.IsPlayerAlive && playerHealth.CurrentHealth <= 1)
        {
            data.low_hp_time_sec += Time.deltaTime;
        }

        // Бездействие после смерти
        if (waitingAfterDeath)
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;

            bool moved = kb != null &&
                (kb.wKey.isPressed || kb.aKey.isPressed || kb.sKey.isPressed || kb.dKey.isPressed);
            bool acted = mouse != null && mouse.leftButton.wasPressedThisFrame;

            if (moved || acted)
                waitingAfterDeath = false;     // игрок начал действовать — стоп
            else
                data.idle_after_death_sec += Time.deltaTime;
        }
    }

    public void RegisterDamageTaken(int amount, int currentHealth)
    {
        if (data == null) return;

        data.damage_taken += amount;
        data.hit_count++;

        if (currentHealth == 1)
            data.near_death_count++;
    }

    public void RegisterDeath(Vector3 position)
    {
        if (data == null) return;

        data.death_count++;

        // Цена ошибки: все, что пройдено после последнего чекпоинта, теряется
        data.progress_lost_percent += Mathf.Max(0f, currentProgressPercent - checkpointProgressPercent);
        data.progress_lost_sec += Mathf.Max(0f, Time.time - safePointTime);

        int zone = GetProgressZone(currentProgressPercent);
        if (zone == lastDeathZone)
        {
            data.repeated_deaths_same_area++;
        }

        lastDeathZone = zone;
    }

    private int GetProgressZone(float percent) => Mathf.Clamp((int)(percent / 25f), 0, 3);

    public void RegisterRespawn()
    {
        if (data == null) return;

        data.restart_count++;
        currentProgressPercent = checkpointProgressPercent;
        safePointTime = Time.time;
        waitingAfterDeath = true;
    }

    // --------Checkpoints----------
    public void RegisterCheckpointReached(int checkpointIndex, float progressPercent)
    {
        if (data == null) return;

        data.checkpoint_reached = Mathf.Max(data.checkpoint_reached, checkpointIndex);

        checkpointProgressPercent = Mathf.Max(checkpointProgressPercent, progressPercent);
        currentProgressPercent = Mathf.Max(currentProgressPercent, checkpointProgressPercent);
        data.max_progress_percent = Mathf.Max(data.max_progress_percent, currentProgressPercent);

        safePointTime = Time.time;
    }

    // ---------Progress-----------
    public void RegisterProgress(float progressPercent)
    {
        if (data == null) return;

        currentProgressPercent = Mathf.Max(currentProgressPercent, progressPercent);
        data.max_progress_percent = Mathf.Max(data.max_progress_percent, currentProgressPercent);
    }

    // ---------Complete-----------
    public void RegisterCompleted()
    {
        if (data == null) return;

        data.max_progress_percent = 100f;
        SaveSession(true, false);
    }
}
