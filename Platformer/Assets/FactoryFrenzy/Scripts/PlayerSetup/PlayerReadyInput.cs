using UnityEngine;
using UnityEngine.InputSystem;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerReadyInput : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.xKey.wasPressedThisFrame)
                playerData.ToggleReadyRpc();
        }
    }
}