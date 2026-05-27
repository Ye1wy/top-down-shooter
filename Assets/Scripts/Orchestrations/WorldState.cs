using System.Collections.Generic;
using UnityEngine;

// Любой расходуемый объект (пикап, спавнер) умеет откатываться
public interface IConsumable
{
    void Rollback();
}

public static class WorldState
{
    private static List<IConsumable> consumedSinceCheckpoint = new List<IConsumable>();

    public static void RegisterConsumed(IConsumable item)
    {
        consumedSinceCheckpoint.Add(item);
    }

    // Чекпоинт: всё израсходованное до этого момента становится постоянным
    public static void Commit()
    {
        consumedSinceCheckpoint.Clear();
    }

    // Смерть: возвращаем всё, что израсходовано после чекпоинта
    public static void Rollback()
    {
        foreach (IConsumable item in consumedSinceCheckpoint)
            item.Rollback();
        consumedSinceCheckpoint.Clear();
    }

    // Перед запуском нового конфига: RuntimeInitialize между сценами не срабатывает,
    // поэтому чистим список явно.
    public static void ResetForNewCondition()
    {
        consumedSinceCheckpoint.Clear();
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        consumedSinceCheckpoint.Clear();
    }
}
