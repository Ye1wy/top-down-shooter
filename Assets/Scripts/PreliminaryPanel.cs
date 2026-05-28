using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Стартовая панель: возрастная группа + частота игры.
// Показывается один раз на участника, потом ответы лежат в PlayerPrefs.
public class PreliminaryPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown ageGroupDropdown;
    [SerializeField] private TMP_Dropdown playFrequencyDropdown;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("Возрастные группы (правь под выборку диссертации)")]
    [SerializeField]
    private string[] ageGroups =
    {
        "18–24",
        "25–34",
        "35–44",
        "45+",
    };

    [Header("Частота игры")]
    [SerializeField]
    private string[] playFrequencies =
    {
        "Не играю",
        "Реже раза в месяц",
        "Несколько раз в месяц",
        "Несколько раз в неделю",
        "Каждый день или почти каждый день",
    };

    [Header("Тексты")]
    [SerializeField] private string ageGroupPlaceholder = "— выберите возрастную группу —";
    [SerializeField] private string playFrequencyPlaceholder = "— выберите частоту —";

    private Action<string, string> onConfirmed;

    private void Awake()
    {
        SetupDropdown(ageGroupDropdown, ageGroups, ageGroupPlaceholder);
        SetupDropdown(playFrequencyDropdown, playFrequencies, playFrequencyPlaceholder);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    public void Show(Action<string, string> callback)
    {
        onConfirmed = callback;

        // Сброс к плейсхолдеру, чтобы пустой выбор было видно.
        if (ageGroupDropdown != null) ageGroupDropdown.SetValueWithoutNotify(0);
        if (playFrequencyDropdown != null) playFrequencyDropdown.SetValueWithoutNotify(0);

        if (errorText != null)
            errorText.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    private void OnConfirmClicked()
    {
        if (ageGroupDropdown == null || playFrequencyDropdown == null)
            return;

        // Индекс 0 — плейсхолдер, его принимать нельзя
        if (ageGroupDropdown.value == 0)
        {
            ShowError("Пожалуйста, выберите возрастную группу.");
            return;
        }
        if (playFrequencyDropdown.value == 0)
        {
            ShowError("Пожалуйста, выберите частоту игры.");
            return;
        }

        string ageGroup = ageGroups[ageGroupDropdown.value - 1];
        string frequency = playFrequencies[playFrequencyDropdown.value - 1];

        AudioManager.Instance?.PlayUIClick();

        // Гасим панель и зовём колбэк через локальную ссылку — повторный клик ничего не сделает
        gameObject.SetActive(false);
        var callback = onConfirmed;
        onConfirmed = null;
        callback?.Invoke(ageGroup, frequency);
    }

    private void SetupDropdown(TMP_Dropdown dropdown, string[] options, string placeholder)
    {
        if (dropdown == null) return;

        dropdown.ClearOptions();
        var list = new List<string> { placeholder };
        list.AddRange(options);
        dropdown.AddOptions(list);
        dropdown.SetValueWithoutNotify(0);
        dropdown.RefreshShownValue();
    }

    private void ShowError(string message)
    {
        if (errorText == null) return;
        errorText.text = message;
        errorText.gameObject.SetActive(true);
    }
}
