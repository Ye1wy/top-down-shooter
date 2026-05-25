// Канонические профили трёх условий эксперимента.
// Значения зашиты в код намеренно: конфигурацию можно сослать в приложении
// диссертации и воспроизвести один-в-один. Тюнинг — здесь, в одном месте.
//
// Логика калибровки: все три рычага (патроны / сильный враг / угроза потери
// прогресса) усиливаются монотонно Easy -> Normal -> Hard. Скачок к Hard
// заведомо больше, чем к Normal, чтобы Normal сел в зону потока, а Hard
// перелетел порог во фрустрацию — это и есть проверяемая строка матрицы.
public static class DifficultyProfiles
{
    public static DifficultyProfile Easy() => new DifficultyProfile
    {
        // Ограничение ресурсов — изобилие, патроны почти не кончаются
        playerStartAmmo = 20,
        ammoPickupAmount = 8,

        // Сильный противник — медленный, редкий, бьёт нечасто
        enemyMoveSpeed = 2.2f,   // держать < PlayerMovement.moveSpeed (5)
        enemyDamage = 1,
        enemyDamageInterval = 1.2f,
        enemyCountMultiplier = 0.7f,
        spawnIntervalMultiplier = 1.3f,  // >1 = реже = легче

        // Угроза потери прогресса — минимальная, активны все чекпоинты
        activeCheckpointCount = -1,
    };

    public static DifficultyProfile Normal() => new DifficultyProfile
    {
        // Дефицит ощутим, но рабочий — целевая «зона потока»
        playerStartAmmo = 12,
        ammoPickupAmount = 5,

        enemyMoveSpeed = 2.8f,
        enemyDamage = 1,
        enemyDamageInterval = 0.9f,
        enemyCountMultiplier = 1.0f,
        spawnIntervalMultiplier = 1.0f,

        // ~половина чекпоинтов — подгони под фактическое число на уровне
        activeCheckpointCount = 2,
    };

    public static DifficultyProfile Hard() => new DifficultyProfile
    {
        // Реальный дефицит: ammo_empty_count в телеметрии должен быть > 0
        playerStartAmmo = 7,
        ammoPickupAmount = 3,

        enemyMoveSpeed = 3.6f,   // всё ещё < 5: кайтинг возможен, смерть «заслужена»
        enemyDamage = 1,      // не поднимать: «2 ошибки = смерть» = дешёвая несправедливость
        enemyDamageInterval = 0.6f,
        enemyCountMultiplier = 1.5f,
        spawnIntervalMultiplier = 0.6f,  // <1 = чаще = плотнее волны

        // Минимум сохранений = максимальная цена ошибки (главный драйвер фрустрации).
        // 1 — жёстко, но проходимо; 0 — полный рестарт на каждой смерти (только для короткого уровня)
        activeCheckpointCount = 1,
    };
}
