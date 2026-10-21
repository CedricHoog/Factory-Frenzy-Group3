using FactoryFrenzy.UI;
using Unity.Netcode;
using UnityEngine;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerLocalSetup : NetworkBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerReadyInput readyInput;
        [SerializeField] private PlayerMovementLock movementLock;
        [SerializeField] private GameObject cameraObject;
        [SerializeField] private Collider collider;

        public override void OnNetworkSpawn()
        {
            if (cameraObject != null) cameraObject.SetActive(IsOwner);
            if (!IsOwner) return;

            readyInput.enabled = true;
            playerData.PlayerPseudo.Value = MainMenu.Pseudo;
            movementLock.MarkReady();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                Physics.IgnoreCollision(collision.collider, collider);
            }
        }

    }
}