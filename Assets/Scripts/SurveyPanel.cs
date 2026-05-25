using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Опрос после каждого конфига. Вопросы зашиты в код: инструмент фиксирован,
// меняешь формулировки — меняешь и анализ. Шкала 1..5.
public class SurveyPanel : MonoBehaviour
{
    [SerializeField] private Transform contentParent;     // Content внутри ScrollView (с VerticalLayoutGroup)
    [SerializeField] private SurveyQuestionRow rowPrefab; // строка: текст + 5 тогглов в одной ToggleGroup
    [SerializeField] private TMP_Text headerText;         // "Конфигурация N из 3"
    [SerializeField] private GameObject warning;          // "Ответьте на все вопросы"

    // 1 — полностью не согласен ... 5 — полностью согласен
    public static readonly (string id, string text)[] Questions =
    {
        ("q01", "Во время прохождения я чувствовал напряжение."),
        ("q02", "Мне было интересно продолжать прохождение."),
        ("q03", "Сложность этой конфигурации казалась мне справедливой."),
        ("q04", "Я чувствовал, что контролирую ситуацию."),
        ("q05", "Во время прохождения я испытывал раздражение или фрустрацию."),
        ("q06", "Мои ошибки казались мне следствием моих собственных действий."),
        ("q07", "Мне хотелось бы попробовать пройти эту конфигурацию ещё раз."),
        ("q08", "Эта конфигурация казалась мне чрезмерно сложной."),
        ("q09", "Ограничение боеприпасов заметно влияло на мои решения."),
        ("q10", "Потеря прогресса после смерти ощущалась значимой."),
        ("q11", "Враги создавали ощущение постоянной угрозы."),
        ("q12", "В этой конфигурации напряжение скорее усиливало интерес, чем мешало играть."),
    };

    private readonly List<SurveyQuestionRow> rows = new();
    private Action<List<SurveyAnswer>> onComplete;

    public void Show(string header, Action<List<SurveyAnswer>> onCompleteCallback)
    {
        onComplete = onCompleteCallback;
        if (headerText != null) headerText.text = header;
        if (warning != null) warning.SetActive(false);

        BuildRows();
        gameObject.SetActive(true);
    }

    private void BuildRows()
    {
        foreach (var r in rows)
            if (r != null) Destroy(r.gameObject);
        rows.Clear();

        foreach (var q in Questions)
        {
            var row = Instantiate(rowPrefab, contentParent);
            row.Setup(q.id, q.text);
            rows.Add(row);
        }
    }

    // Привязать к кнопке "Готово"
    public void OnSubmit()
    {
        var answers = new List<SurveyAnswer>();
        foreach (var row in rows)
        {
            int v = row.Value;
            if (v == 0)                          // есть неотвеченный пункт
            {
                if (warning != null) warning.SetActive(true);
                return;
            }
            answers.Add(new SurveyAnswer { question_id = row.QuestionId, value = v });
        }

        gameObject.SetActive(false);
        onComplete?.Invoke(answers);
    }
}
