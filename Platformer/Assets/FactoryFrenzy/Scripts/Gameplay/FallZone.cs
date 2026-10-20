
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FallZone : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "[FALLZONE] Collider detected: " +
            other.name,
            this
        );

        PlayerRespawn player =
            other.GetComponentInParent<PlayerRespawn>();

        if (player == null)
        {
            Debug.LogWarning(
                "[FALLZONE] No PlayerRespawn found.",
                this
            );
            return;
        }

        if (!player.HasMovementAuthority())
        {
            Debug.LogWarning(
                "[FALLZONE] No movement authority. " +
                "IsServer=" + player.IsServer +
                ", IsOwner=" + player.IsOwner,
                this
            );
            return;
        }

        Debug.Log(
            "[FALLZONE] Respawning player.",
            this
        );

        player.Respawn();
    }
}
