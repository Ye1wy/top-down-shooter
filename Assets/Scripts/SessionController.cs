using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Живёт в той же сцене уровня (одна сцена на весь эксперимент).
// Управляет контрбалансировкой, последовательностью условий, опросом и единым файлом.
public class SessionController : MonoBehaviour
{
    public static SessionController Instance { get; private set; }

    [Serializable]
    public class ConditionSetup
    {
        public Difficult.DifficultType condition;
        public DifficultyProfile profile;
    }

    [Header("Участник")]
    [SerializeField] private string participantId = "P001";
    [SerializeField] private int participantNumber = 1;   // увеличивай на каждого участника

    [Header("Конфигурации (канонический порядок, например Easy, Normal, Hard)")]
    [SerializeField] private ConditionSetup[] conditions = new ConditionSetup[3];
    
    [Header("Ссылки в сцене")]
    [SerializeField] private GameFlowManager gameFlow;
    [SerializeField] private SurveyPanel surveyPanel;
    [SerializeField] private GameObject finishedPanel;

    // 6 перестановок индексов {0,1,2} — полная контрбалансировка.
    // participant_number 1..6 → перестановки 0..5, далее по кругу.
    private static readonly int[][] Permutations =
    {
        new[] { 0, 1, 2 },
        new[] { 0, 2, 1 },
        new[] { 1, 0, 2 },
        new[] { 1, 2, 0 },
        new[] { 2, 0, 1 },
        new[] { 2, 1, 0 },
    };

    private ParticipantData participant;
    private int[] order;          // последовательность индексов в conditions
    private int currentStep;      // 0..2
    private string filePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        int permIndex = (Mathf.Max(1, participantNumber) - 1) % Permutations.Length;
        order = Permutations[permIndex];

        participant = new ParticipantData
        {
            participant_id = participantId,
            participant_number = participantNumber,
            permutation_index = permIndex,
            experiment_start = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
        };
        foreach (int idx in order)
            participant.sequence.Add(conditions[idx].condition.ToString());

        string fileName = $"telemetry_{participantId}_{participant.experiment_start}.json";
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        currentStep = 0;
    }

    private void Start()
    {
        if (surveyPanel != null) surveyPanel.gameObject.SetActive(false);
        if (finishedPanel != null) finishedPanel.SetActive(false);
        BeginCurrentCondition();
    }

    public Difficult.DifficultType CurrentCondition => conditions[order[currentStep]].condition;
    public int CurrentOrderIndex => currentStep + 1;
    public int TotalConditions => order.Length;

    private void BeginCurrentCondition()
    {
        var setup = conditions[order[currentStep]];
        gameFlow.BeginCondition(setup.profile);
    }

    // Вызывает GameFlowManager сцены, когда условие закончился (финиш или сдача).
    public void OnConditionEnded(TelemetrySessionData stats)
    {
        Time.timeScale = 0f; // замораживаем игру под опрос

        string header = $"Конфигурация {CurrentOrderIndex} из {TotalConditions}";
        surveyPanel.Show(header, answers => OnSurveyCompleted(stats, answers));
    }

    private void OnSurveyCompleted(TelemetrySessionData stats, List<SurveyAnswer> answers)
    {
        participant.runs.Add(new ConditionRun
        {
            condition_id = CurrentCondition.ToString(),
            order_index = CurrentOrderIndex,
            stats = stats,
            survey = answers,
        });

        WriteFile(); // инкрементально: данные на диске уже после первого конфига

        currentStep++;
        if (currentStep < order.Length)
            BeginCurrentCondition();
        else
            ShowFinished();
    }

    private void WriteFile()
    {
        try
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(participant, true));
            Debug.Log($"Телеметрия записана: {filePath} (прогонов: {participant.runs.Count})");
        }
        catch (Exception e)
        {
            Debug.LogError($"Не удалось записать телеметрию: {e.Message}");
        }
    }

    private void ShowFinished()
    {
        Time.timeScale = 0f;
        if (finishedPanel != null) finishedPanel.SetActive(true);
        Debug.Log("Эксперимент завершён.");
    }

    // Привязать к кнопке "Выйти" на финальном экране
    public void QuitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
