// Запасной кодовый вариант профилей.
// Основной путь для балансировки теперь — DifficultConfig asset через инспектор.
public static class DifficultyProfiles
{
    public static DifficultyProfile Easy() => new DifficultyProfile
    {
        playerStartAmmo = 20,

        enemyMoveSpeed = 2.2f,
        enemyDamage = 1,
        enemyDamageInterval = 1.2f,
    };

    public static DifficultyProfile Normal() => new DifficultyProfile
    {
        playerStartAmmo = 12,

        enemyMoveSpeed = 2.8f,
        enemyDamage = 1,
        enemyDamageInterval = 0.9f,
    };

    public static DifficultyProfile Hard() => new DifficultyProfile
    {
        playerStartAmmo = 7,

        enemyMoveSpeed = 3.6f,
        enemyDamage = 1,
        enemyDamageInterval = 0.6f,
    };

    public static DifficultyProfile Create(Difficult.DifficultType difficulty)
    {
        return difficulty switch
        {
            Difficult.DifficultType.Easy => Easy(),
            Difficult.DifficultType.Normal => Normal(),
            Difficult.DifficultType.Hard => Hard(),
            _ => Normal()
        };
    }

    private static DifficultyProfile DefaultEasy() => new DifficultyProfile
    {
        playerStartAmmo = 20,
        enemyMoveSpeed = 2.2f,
        enemyDamage = 1,
        enemyDamageInterval = 1.2f,
    };

    private static DifficultyProfile DefaultNormal() => new DifficultyProfile
    {
        playerStartAmmo = 12,
        enemyMoveSpeed = 2.8f,
        enemyDamage = 1,
        enemyDamageInterval = 0.9f,
    };

    private static DifficultyProfile DefaultHard() => new DifficultyProfile
    {
        playerStartAmmo = 7,
        enemyMoveSpeed = 3.6f,
        enemyDamage = 1,
        enemyDamageInterval = 0.6f,
    };
}
