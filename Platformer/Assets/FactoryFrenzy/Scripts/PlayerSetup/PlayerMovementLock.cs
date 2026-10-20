using System;
using SUPERCharacter;
using Unity.Netcode;
using UnityEngine;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerMovementLock : NetworkBehaviour
    {
        [SerializeField] private SUPERCharacterAIO character;
        [SerializeField] private Rigidbody body;

        public static bool IsLocked { get; private set; }
        private static event Action<bool> OnLockChanged;

        private bool _ready;

        public static void SetMovementLocked(bool locked)
        {
            IsLocked = locked;
            OnLockChanged?.Invoke(locked);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) OnLockChanged += Apply;
        }

        public override void OnNetworkDespawn() => OnLockChanged -= Apply;

        public void MarkReady()
        {
            _ready = true;
            character.enabled = true;
            Apply(IsLocked);
        }

        private void Apply(bool locked)
        {
            if (!_ready) return;

            // La caméra n'est pas touchée : seuls le déplacement et le saut sont coupés
            character.enableMovementControl = !locked;
            character.canJump = !locked;

            if (locked)
            {
                // MovePlayer ne tourne plus, donc plus rien ne freine le joueur : on l'arrête
                Vector3 v = body.linearVelocity;
                body.linearVelocity = new Vector3(0f, v.y, 0f);
                character.isIdle = true;
            }
        }
    }
}