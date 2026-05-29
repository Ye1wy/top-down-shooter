using UnityEngine;
using UnityEngine.InputSystem;

// По одному на каждую игровую сцену. Управляет стартовым меню, паузой/сдачей,
// завершением конфига. Ссылки на игрока — локальные для сцены, проблем нет.
public class GameFlowManager : MonoBehaviour
{
    [Header("Панели")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Сдача (новое)")]
    [SerializeField] private GameObject inGameHud;         // HUD с кнопкой "Завершить", виден во время игры
    [SerializeField] private GameObject confirmGiveUpPanel; // Модал "Вы уверены?"

    [Header("Ссылки на игрока")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private LevelController levelController;

    private bool isPlaying = false;
    private bool isPaused = false;

    private void Awake()
    {
        // Старт конфига: игра заморожена, ждём кнопку "Начать"
        Time.timeScale = 0f;

        if (startPanel != null) startPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (inGameHud != null) inGameHud.SetActive(false);
        if (confirmGiveUpPanel != null) confirmGiveUpPanel.SetActive(false);

        SetPlayerControl(false);
        isPlaying = false;
    }

    // Вызывает SessionController перед каждым условием
    public void BeginCondition(DifficultyProfile profile)
    {
        if (levelController != null)
            levelController.ApplyConditionAndReset(profile);

        isPlaying = false;
        isPaused = false;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
        if (inGameHud != null) inGameHud.SetActive(false);
        if (confirmGiveUpPanel != null) confirmGiveUpPanel.SetActive(false);
        SetPlayerControl(false);
    }

    private void Update()
    {
        if (!isPlaying) return;

        Keyboard kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            TogglePause();
    }

    // --- Кнопки UI ---

    // Кнопка "Начать"
    public void StartGame()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (inGameHud != null) inGameHud.SetActive(true);
        SetPlayerControl(true);
        Time.timeScale = 1f;
        isPlaying = true;

        AudioManager.Instance?.PlayMusic();

        if (TelemetryManager.Instance != null)
            TelemetryManager.Instance.BeginSession();
    }

    public void TogglePause()
    {
        if (!isPlaying) return;

        isPaused = !isPaused;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;   // время паузы не идёт в duration/прочие метрики
        SetPlayerControl(!isPaused);
    }

    // Кнопка "Продолжить" в паузе
    public void Resume()
    {
        if (isPaused) TogglePause();
    }

    // Кнопка "Сдаться" в паузе → конфиг засчитывается как quit, но участник идёт на опрос и дальше
    public void GiveUp()
    {
        EndCondition(false, true);
    }

    // Вызывается FinishTrigger
    public void Finish()
    {
        EndCondition(true, false);
    }

    // --- Внутреннее ---

    private void EndCondition(bool completed, bool quit)
    {
        if (!isPlaying) return;
        isPlaying = false;
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        SetPlayerControl(false);

        AudioManager.Instance?.StopMusic();

        TelemetrySessionData stats = null;
        if (TelemetryManager.Instance != null)
            stats = TelemetryManager.Instance.EndCondition(completed, quit);

        if (SessionController.Instance != null && stats != null)
            SessionController.Instance.OnConditionEnded(stats);
    }

    public void RequestGiveUp()
    {
        if (!isPlaying) return;

        // Паузим, но не показываем стандартную панель паузы — только confirm
        Time.timeScale = 0f;
        SetPlayerControl(false);
        if (confirmGiveUpPanel != null) confirmGiveUpPanel.SetActive(true);
        if (inGameHud != null) inGameHud.SetActive(false);
    }

    public void CancelGiveUp()
    {
        if (confirmGiveUpPanel != null) confirmGiveUpPanel.SetActive(false);
        if (inGameHud != null) inGameHud.SetActive(true);
        Time.timeScale = 1f;
        SetPlayerControl(true);
    }

    public void ConfirmGiveUp()
    {
        if (confirmGiveUpPanel != null) confirmGiveUpPanel.SetActive(false);
        GiveUp(); // существующий метод — засчитает quit, покажет опрос
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerMovement != null) playerMovement.enabled = enabled;
        if (playerShooting != null) playerShooting.enabled = enabled;
    }
}
