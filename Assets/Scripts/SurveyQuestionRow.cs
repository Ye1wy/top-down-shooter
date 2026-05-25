using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Одна строка опроса: текст вопроса + ровно 5 тогглов (в одной ToggleGroup),
// слева направо значения 1..5.
public class SurveyQuestionRow : MonoBehaviour
{
    [SerializeField] private TMP_Text questionLabel;
    [SerializeField] private Toggle[] toggles;   // ровно 5, по порядку 1..5

    public string QuestionId { get; private set; }

    public void Setup(string questionId, string questionText)
    {
        QuestionId = questionId;
        if (questionLabel != null)
            questionLabel.text = questionText;

        foreach (var t in toggles)
            if (t != null) t.isOn = false;
    }

    // 0 — ничего не выбрано, иначе 1..5
    public int Value
    {
        get
        {
            for (int i = 0; i < toggles.Length; i++)
                if (toggles[i] != null && toggles[i].isOn)
                    return i + 1;
            return 0;
        }
    }
}
