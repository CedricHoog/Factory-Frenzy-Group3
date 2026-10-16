using System;
using System.Reflection;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerMovementLock : NetworkBehaviour
    {
        [SerializeField] private StarterAssetsInputs inputs;
        [SerializeField] private ThirdPersonController controller;
        [SerializeField] private Animator animator;

        public static bool IsLocked { get; private set; }
        private static event Action<bool> OnLockChanged;

        private bool _ready;

        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly FieldInfo YawField = typeof(ThirdPersonController).GetField("_cinemachineTargetYaw", Flags);
        private static readonly FieldInfo PitchField = typeof(ThirdPersonController).GetField("_cinemachineTargetPitch", Flags);

        public static void SetMovementLocked(bool locked)
        {
            IsLocked = locked;
            OnLockChanged?.Invoke(locked);
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner) OnLockChanged += Apply;
        }

        public override void OnNetworkDespawn()
        {
            OnLockChanged -= Apply;
        }

        public void MarkReady()
        {
            _ready = true;
            controller.enabled = !IsLocked;
        }

        private void Apply(bool locked)
        {
            if (!_ready) return;

            if (locked)
            {
                controller.enabled = false;
                inputs.MoveInput(Vector2.zero);
                inputs.JumpInput(false);
                inputs.SprintInput(false);
                ResetAnimator();
            }
            else
            {
                ResetCameraAndPhysics();
                controller.enabled = true;
            }
        }

        private void ResetAnimator()
        {
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("MotionSpeed", 1f);
            animator.SetBool("Jump", false);
            animator.SetBool("FreeFall", false);
            animator.SetBool("Grounded", true);
        }

        private void ResetCameraAndPhysics()
        {
            YawField.SetValue(controller, 0f);
            PitchField.SetValue(controller, 0f);
        }
    }
}