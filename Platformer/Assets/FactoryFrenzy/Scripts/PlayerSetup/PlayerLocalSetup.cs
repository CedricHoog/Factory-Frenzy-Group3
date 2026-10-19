using FactoryFrenzy.UI;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerLocalSetup : NetworkBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ThirdPersonController thirdPersonController;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private StarterAssetsInputs starterInputs;
        [SerializeField] private PlayerCursorLock cursorLock;
        [SerializeField] private PlayerReadyInput readyInput;
        [SerializeField] private PlayerMovementLock movementLock;

        private static readonly System.Reflection.FieldInfo MainCameraField =
            typeof(ThirdPersonController).GetField("_mainCamera",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        private static readonly System.Reflection.FieldInfo ControllerField =
            typeof(ThirdPersonController).GetField("_controller",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        public override void OnNetworkSpawn()
        {
            ControllerField.SetValue(thirdPersonController, GetComponent<CharacterController>());

            if (!IsOwner) return;

            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
            StartCoroutine(SetupPlayer());
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && NetworkManager != null && NetworkManager.SceneManager != null)
                NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }

        private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            StopAllCoroutines();
            StartCoroutine(SetupPlayer());
        }

        private IEnumerator SetupPlayer()
        {
            for (int i = 0; i < 300; i++)
            {
                var cam = FindAnyObjectByType<Unity.Cinemachine.CinemachineCamera>();
                var mainCam = Camera.main;

                if (cam != null && mainCam != null)
                {
                    cam.Follow = cameraRoot;
                    cam.LookAt = cameraRoot;

                    MainCameraField.SetValue(thirdPersonController, mainCam.gameObject);

                    playerInput.enabled = true;
                    starterInputs.enabled = true;
                    cursorLock.enabled = true;
                    readyInput.enabled = true;
                    playerData.PlayerPseudo.Value = MainMenu.Pseudo;
                    movementLock.MarkReady();
                    yield break;
                }
                yield return null;
            }
        }
    }
}