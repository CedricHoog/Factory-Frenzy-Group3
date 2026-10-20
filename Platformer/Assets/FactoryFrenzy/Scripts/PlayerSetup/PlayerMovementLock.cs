using System;
using Unity.Netcode;
using UnityEngine;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerMovementLock : NetworkBehaviour
    {
        [SerializeField] private Behaviour controller;
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
            Apply(IsLocked);
        }

        private void Apply(bool locked)
        {
            if (!_ready) return;
            controller.enabled = !locked;
            if (locked && body != null && !body.isKinematic)
                body.linearVelocity = Vector3.zero;
        }
    }
}