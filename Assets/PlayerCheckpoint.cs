using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour
{
    private void Start()
    {
        // Был чекпоинт в этой сессии — встаём на него, иначе остаёмся на старте
        if (CheckpointData.HasCheckpoint)
        {
            transform.position = new Vector3(
                CheckpointData.Position.x,
                CheckpointData.Position.y,
                transform.position.z);
        }
    }
}
