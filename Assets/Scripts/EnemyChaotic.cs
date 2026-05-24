using UnityEngine;

public class EnemyChaotic : EnemyBase
{
    [Header("Хаотичное движение")]
    [SerializeField] private float wanderStrength = 60f;
    [SerializeField] private float wanderFrequency = 1.5f;

    private float noiseSeed;

    protected override void Awake()
    {
        base.Awake(); // обязательно: иначе базовая инициализация (rb) не выполнится
        noiseSeed = Random.value * 100f;
    }

    protected override Vector2 GetMoveDirection()
    {
        Vector2 toPlayer = DirectionToPlayer();
        float noise = Mathf.PerlinNoise(noiseSeed, Time.time * wanderFrequency) * 2f - 1f;
        return Rotate(toPlayer, noise * wanderStrength);
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
