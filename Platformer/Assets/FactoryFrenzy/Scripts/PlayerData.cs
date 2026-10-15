using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerData : NetworkBehaviour
    {
        public PlayerInput PlayerInput;
        public StarterAssetsInputs StarterInputs;
        public ThirdPersonController ThirdPersonController;
        public Transform CameraRoot;

        private static readonly System.Reflection.FieldInfo MainCameraField =
            typeof(ThirdPersonController).GetField("_mainCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo ControllerField =
            typeof(ThirdPersonController).GetField("_controller",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo VerticalVelocityField =
            typeof(ThirdPersonController).GetField("_verticalVelocity",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public void Bounce(float velocity)
        {
            if (!IsOwner) return;
            VerticalVelocityField.SetValue(ThirdPersonController, velocity);
        }

        private void Update()
        {
            if (!IsOwner || !ThirdPersonController.enabled)
            {
                return;
            }

            bool locked = Cursor.lockState == CursorLockMode.Locked;

            StarterInputs.cursorInputForLook = locked;

            if (!locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public override void OnNetworkSpawn()
        {
            ControllerField.SetValue(ThirdPersonController, GetComponent<CharacterController>());

            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;

            StartCoroutine(SetupCameraRoutine());
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (NetworkManager != null && NetworkManager.SceneManager != null)
                    NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            StopAllCoroutines();
            StartCoroutine(SetupCameraRoutine());
        }

        private IEnumerator SetupCameraRoutine()
        {
            for (int i = 0; i < 300; i++)
            {
                var cam = FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
                var mainCam = Camera.main;

                if (cam != null && mainCam != null)
                {
                    cam.Follow = CameraRoot;
                    cam.LookAt = CameraRoot;

                    // Donne au ThirdPersonController la caméra du Lobby, sans modifier son code
                    MainCameraField.SetValue(ThirdPersonController, mainCam.gameObject);

                    // Activation seulement maintenant que tout est prêt
                    PlayerInput.enabled = true;
                    StarterInputs.enabled = true;
                    ThirdPersonController.enabled = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    yield break;
                }
                yield return null;
            }

            Debug.LogWarning("PlayerData : CinemachineCamera ou MainCamera introuvable dans la scène.");
        }

        [ClientRpc]
        public void TeleportClientRpc(Vector3 position, Quaternion rotation)
        {
            if (!IsOwner) return;

            TryGetComponent<CharacterController>(out var cc);
            if (cc != null) cc.enabled = false;

            if (TryGetComponent<Unity.Netcode.Components.NetworkTransform>(out var nt))
                nt.Teleport(position, rotation, transform.localScale);
            else
                transform.SetPositionAndRotation(position, rotation);

            if (cc != null) cc.enabled = true;
        }
    }
}