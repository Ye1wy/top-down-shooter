using System;

[Serializable]
public class TelemetrySessionData
{
    // Контекст
    public string participant_id;
    public string condition_id;
    public int order_index;

    // Исход
    public bool completed;
    public bool quit;
    public float duration_sec;
    public float max_progress_percent;

    // Смерти
    public int death_count;
    public int restart_count;

    // Ресурсы
    public int shots_fired;
    public int shots_hit;
    public float accuracy;
    public int ammo_left;
    public int ammo_empty_count;

    // Угроза
    public int damage_taken;
    public int hit_count;
    public float low_hp_time_sec;
    public int near_death_count;

    // Потеря прогресса
    public int checkpoint_reached;          // самый дальний достигнутый чекпоинт
    public float progress_lost_sec;
    public float progress_lost_percent;

    // Фрустрация
    public int repeated_deaths_same_area;
    public float idle_after_death_sec;
}
