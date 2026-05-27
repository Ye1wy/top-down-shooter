using UnityEngine;

public class EnemyChaser : EnemyBase
{
    protected override Vector2 GetMoveDirection()
    {
        return DirectionToPlayer();
    }
}
