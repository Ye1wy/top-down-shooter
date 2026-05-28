using System;
using System.Collections.Generic;

// Верхний уровень: один файл = один участник, все три прогона + ответы опроса.
[Serializable]
public class ParticipantData
{
    public string participant_id;
    public int participant_number;     // 1-based, задаёт перестановку
    public int permutation_index;      // какая из 6 перестановок выпала
    public string age_group;
    public string play_frequency;
    public string experiment_start;    // yyyy-MM-dd_HH-mm-ss (фиксирует имя файла)
    public List<string> sequence = new();      // condition_id в порядке прохождения
    public List<ConditionRun> runs = new();
}

// Один прогон одной конфигурации.
[Serializable]
public class ConditionRun
{
    public string condition_id;        // Easy / Normal / Hard
    public int order_index;            // позиция в последовательности участника (1..3)
    public TelemetrySessionData stats; // вся поведенческая телеметрия прогона
    public List<SurveyAnswer> survey = new();
}

// Один ответ на пункт опроса.
[Serializable]
public class SurveyAnswer
{
    public string question_id;         // q01..q12
    public int value;                  // 1..5
}
