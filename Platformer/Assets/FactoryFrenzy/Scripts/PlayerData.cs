using StarterAssets;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerData : NetworkBehaviour
    {
        public PlayerInput PlayerInput;
        public StarterAssetsInputs StarterInputs;
        public ThirdPersonController ThirdPersonController;
        public PlayerCursorLock CursorLock;

        public void EnableLocalControl()
        {
            if (!IsOwner) return;

            PlayerInput.enabled = true;
            StarterInputs.enabled = true;
            ThirdPersonController.enabled = true;
            CursorLock.enabled = true;
        }
    }
}