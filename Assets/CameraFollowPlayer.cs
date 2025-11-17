using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;

    // offset en el espacio LOCAL del player
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);

    void LateUpdate()
    {
        // convertir offset local → offset real según la rotación del player
        Vector3 desiredPosition = player.TransformPoint(offset);

        transform.position = desiredPosition;

        // mirar al jugador
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}