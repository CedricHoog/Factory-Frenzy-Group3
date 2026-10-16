using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerCursorLock : NetworkBehaviour
    {
        [SerializeField] private StarterAssetsInputs inputs;

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            inputs.cursorInputForLook = locked;

            if (!locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}