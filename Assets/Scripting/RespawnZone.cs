using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    public Transform respawnTransform;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && player.lastCheckpoint != respawnTransform)
        {
            player.lastCheckpoint = respawnTransform;
            Debug.Log("Checkpoint aggiornato!");
        }
    }
}
