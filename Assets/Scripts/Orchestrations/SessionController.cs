using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Живёт в той же сцене уровня (одна сцена на весь эксперимент).
// Веб-версия: участник назначается СЕРВЕРОМ (уникальный id + номер для
// контрбалансировки), телеметрия уходит на сервер POST-ом после каждого условия.
public class SessionController : MonoBehaviour
{
    public static SessionController Instance { get; private set; }

    [Header("Сервер")]
    [SerializeField] private string serverUrl = "https://your.domain";  // тот же origin, что и WebGL
    [SerializeField] private string studyToken = "change-me";           // должен совпадать со STUDY_TOKEN сервера

    [Header("Конфигурации (канонический порядок, например Easy, Normal, Hard)")]
    [SerializeField] private DifficultConfig[] conditions = new DifficultConfig[3];

    [Header("Ссылки в сцене")]
    [SerializeField] private GameFlowManager gameFlow;
    [SerializeField] private SurveyPanel surveyPanel;
    [SerializeField] private GameObject finishedPanel;
    [SerializeField] private GameObject loadingPanel;   // опционально: «Загрузка…», пока идёт назначение

    [Header("Offline-фоллбэк (только для теста в редакторе без сервера)")]
    [SerializeField] private int offlineParticipantNumber = 1;

    // 6 перестановок индексов {0,1,2} — полная контрбалансировка.
    private static readonly int[][] Permutations =
    {
        new[] { 0, 1, 2 },
        new[] { 0, 2, 1 },
        new[] { 1, 0, 2 },
        new[] { 1, 2, 0 },
        new[] { 2, 0, 1 },
        new[] { 2, 1, 0 },
    };

    // Ключи для запоминания назначения между перезагрузками страницы (рефреш не должен
    // сжигать новый номер контрбалансировки и плодить дубль-обрывки).
    private const string PrefId = "study_participant_id";
    private const string PrefNum = "study_participant_number";

    private ParticipantData participant;
    private int[] order;          // последовательность индексов в conditions
    private int currentStep;      // 0..2

    private string assignedId;
    private int assignedNumber;

    [Serializable]
    private class AssignResponse
    {
        public int participant_number;
        public string participant_id;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (surveyPanel != null) surveyPanel.gameObject.SetActive(false);
        if (finishedPanel != null) finishedPanel.SetActive(false);
        StartCoroutine(InitializeAndRun());
    }

    public Difficult.DifficultType CurrentCondition => conditions[order[currentStep]].difficult;
    public int CurrentOrderIndex => currentStep + 1;
    public int TotalConditions => order.Length;

    // Получаем участника (с сервера или из сохранённого назначения), затем стартуем.
    private IEnumerator InitializeAndRun()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // Рефреш страницы: продолжаем тем же участником, новый номер не запрашиваем.
        if (PlayerPrefs.HasKey(PrefId) && PlayerPrefs.HasKey(PrefNum))
        {
            assignedId = PlayerPrefs.GetString(PrefId);
            assignedNumber = PlayerPrefs.GetInt(PrefNum);
            Debug.Log($"Возобновление: тот же участник {assignedId} (#{assignedNumber}).");
        }
        else
        {
            yield return RequestAssignment();
        }

        BuildParticipant();
        currentStep = 0;

        if (loadingPanel != null) loadingPanel.SetActive(false);
        BeginCurrentCondition();
    }

    private IEnumerator RequestAssignment()
    {
        string url = serverUrl.TrimEnd('/') + "/assign";
        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Study-Token", studyToken);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var resp = JsonUtility.FromJson<AssignResponse>(req.downloadHandler.text);
                assignedId = resp.participant_id;
                assignedNumber = resp.participant_number;

                PlayerPrefs.SetString(PrefId, assignedId);
                PlayerPrefs.SetInt(PrefNum, assignedNumber);
                PlayerPrefs.Save();

                Debug.Log($"Назначен участник {assignedId} (#{assignedNumber}).");
            }
            else
            {
                // Сервер недоступен — offline-режим для отладки в редакторе.
                // В реальном вебе сюда попадать не должны: это видно по случайному id.
                assignedId = Guid.NewGuid().ToString();
                assignedNumber = Mathf.Max(1, offlineParticipantNumber);
                Debug.LogWarning($"Сервер недоступен ({req.error}). OFFLINE-режим, " +
                                 $"данные НЕ уйдут на сервер. id={assignedId}, #{assignedNumber}.");
            }
        }
    }

    private void BuildParticipant()
    {
        int permIndex = (Mathf.Max(1, assignedNumber) - 1) % Permutations.Length;
        order = Permutations[permIndex];

        participant = new ParticipantData
        {
            participant_id = assignedId,
            participant_number = assignedNumber,
            permutation_index = permIndex,
            experiment_start = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
        };
        foreach (int idx in order)
            participant.sequence.Add(conditions[idx].difficult.ToString());
    }

    private void BeginCurrentCondition()
    {
        var config = conditions[order[currentStep]];
        if (config == null)
        {
            Debug.LogError($"SessionController: не назначен DifficultConfig для индекса {order[currentStep]}.");
            return;
        }
        gameFlow.BeginCondition(config.profile);

    }

    // Вызывает GameFlowManager, когда условие закончилось (финиш или сдача).
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

        // Инкрементально: после каждого условия шлём весь объект — данные на сервере
        // уже после первого прохода, даже если участник дальше бросит.
        StartCoroutine(SendTelemetry());

        currentStep++;
        if (currentStep < order.Length)
            BeginCurrentCondition();
        else
            ShowFinished();
    }

    private IEnumerator SendTelemetry()
    {
        string url = serverUrl.TrimEnd('/') + "/telemetry";
        string json = JsonUtility.ToJson(participant);

        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Study-Token", studyToken);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Не удалось отправить телеметрию: {req.error}");
            else
                Debug.Log($"Телеметрия отправлена (прогонов: {participant.runs.Count}).");
        }
    }

    private void ShowFinished()
    {
        Time.timeScale = 0f;
        if (finishedPanel != null) finishedPanel.SetActive(true);
        Debug.Log("Эксперимент завершён.");
    }

    // Привязать к кнопке «Выйти» на финальном экране
    public void QuitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
