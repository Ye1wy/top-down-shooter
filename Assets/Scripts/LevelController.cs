using UnityEngine;

// Мягкий рестарт уровня на месте (без перезагрузки сцены) + применение профиля сложности.
// Вызывается перед каждым условием.
public class LevelController : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShooting playerShooting;

    public void ApplyConditionAndReset(DifficultyProfile profile)
    {
        // Профиль увидят все враги/спавнеры/пикапы, которые появятся в этом условии
        DifficultyState.Current = profile;

        // Статика (между условиями RuntimeInitialize не срабатывает — чистим явно)
        CheckpointData.Reset();
        CheckpointData.SavedAmmo = profile.playerStartAmmo;
        WorldState.ResetForNewCondition();
        PlayerHealth.ResetAlive();

        // Убираем врагов, оставшихся с прошлого условия
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            Destroy(enemy);

        // Спавнеры и пикапы — в исходное состояние (Rollback = "не израсходован")
        foreach (var spawner in FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            spawner.Rollback();

            var config = profile.FindSpawner(spawner.BalanceId);
            if (config != null)
                spawner.ApplyBalance(config);
        }

        foreach (var ammo in FindObjectsByType<AmmoPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ammo.Rollback();

            var config = profile.FindAmmoPickup(ammo.BalanceId);
            if (config != null)
                ammo.ApplyBalance(config);
        }

        // Финишный триггер — снять флаг, иначе на 2-м условии финиш не сработает
        var finish = FindFirstObjectByType<FinishTrigger>(FindObjectsInactive.Include);
        if (finish != null)
            finish.ResetTrigger();

        // Чекпоинты: сброс флага + включаем первые N по индексу, остальные гасим
        foreach (var checkpoint in FindObjectsByType<Checkpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            checkpoint.ResetForNewCondition();

            var config = profile.FindCheckpoint(checkpoint.BalanceId);
            if (config != null)
                checkpoint.ApplyBalance(config);
        }

        // Игрок: на старт, полное HP, боезапас по профилю
        if (playerHealth != null)
            playerHealth.ResetForNewCondition();

        if (playerShooting != null)
            playerShooting.SetAmmo(profile.playerStartAmmo);
    }
}
