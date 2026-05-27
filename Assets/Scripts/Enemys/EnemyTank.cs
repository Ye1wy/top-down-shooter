using System.Collections;
using UnityEngine;

public class EnemyTank : EnemyBase
{
    [Header("AOE-удар")]
    [SerializeField] private float attackRange = 2.0f;      // дистанция, на которой начинает заряжать
    [SerializeField] private float attackRadius = 2.5f;     // радиус самого удара
    [SerializeField] private float chargeTime = 1.2f;       // длительность телеграфа (окно для уклонения)
    [SerializeField] private float recoveryTime = 0.8f;     // кулдаун после удара (окно для контратаки)
    [SerializeField] private int strikeDamage = 2;          // обычно больше базового enemyDamage

    [Header("Визуальный телеграф")]
    [SerializeField] private GameObject telegraphIndicator; // дочерний объект — полупрозрачный круг

    private enum State { Chasing, Charging, Recovering }
    private State state = State.Chasing;

    protected override void Start()
    {
        base.Start();
        ShowTelegraph(false);
        StartCoroutine(AttackLoop());
    }

    // Танк ходит только в фазе преследования. В Charging/Recovering — стоит.
    // Это и есть «окно для игрока»: видишь телеграф — отходи, видишь recovery — стреляй.
    protected override Vector2 GetMoveDirection()
    {
        return state == State.Chasing ? DirectionToPlayer() : Vector2.zero;
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            // Ждём, пока игрок жив и подошёл достаточно близко
            while (player == null || !PlayerHealth.IsPlayerAlive ||
                   Vector2.Distance(rb.position, player.position) > attackRange)
                yield return null;

            // Charging — выдаём телеграф, даём игроку шанс выйти из радиуса
            state = State.Charging;
            ShowTelegraph(true);
            yield return new WaitForSeconds(chargeTime);

            // Удар: «успел/не успел» решается тем, кто остался в круге к этому моменту
            DoAOEStrike();
            ShowTelegraph(false);

            // Recovering — стоим, не атакуем. Игрок может безопасно стрелять.
            state = State.Recovering;
            yield return new WaitForSeconds(recoveryTime);

            state = State.Chasing;
        }
    }

    private void DoAOEStrike()
    {
        if (!PlayerHealth.IsPlayerAlive) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, attackRadius);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            if (col.TryGetComponent<PlayerHealth>(out var health))
                health.TakeDamage(strikeDamage);
            break;     // один игрок — одна порция урона за удар
        }
    }

    private void ShowTelegraph(bool visible)
    {
        if (telegraphIndicator == null) return;
        telegraphIndicator.SetActive(visible);
        if (visible)
            telegraphIndicator.transform.localScale = Vector3.one * (attackRadius * 2f);
    }

    // Визуальная отладка в редакторе: видны и зона "начать заряжать", и фактический радиус удара
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
