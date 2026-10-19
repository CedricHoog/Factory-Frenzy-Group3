using System.Collections;
using System.Collections.Generic;
using FactoryFrenzy.Gameplay;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingGame : NetworkBehaviour
{
    [SerializeField] private GameObject countdownCanvas;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int duration = 3;

    private readonly NetworkVariable<bool> _frozen = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> _secondsLeft = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        _frozen.OnValueChanged += OnFrozenChanged;
        _secondsLeft.OnValueChanged += OnSecondsChanged;

        PlayerMovementLock.SetMovementLocked(_frozen.Value);
        UpdateUI(_secondsLeft.Value);

        if (IsServer)
        {
            GameState.GameStarted = true;

            _frozen.Value = true;
            PlayerMovementLock.SetMovementLocked(true);
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }

    public override void OnNetworkDespawn()
    {
        _frozen.OnValueChanged -= OnFrozenChanged;
        _secondsLeft.OnValueChanged -= OnSecondsChanged;
        PlayerMovementLock.SetMovementLocked(false);

        if (IsServer && NetworkManager != null && NetworkManager.SceneManager != null)
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    private void OnFrozenChanged(bool oldValue, bool newValue) =>
        PlayerMovementLock.SetMovementLocked(newValue);

    private void OnSecondsChanged(int oldValue, int newValue) => UpdateUI(newValue);

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        StartCoroutine(CountdownRoutine());
    }
    private IEnumerator CountdownRoutine()
    {
        for (int s = duration; s > 0; s--)
        {
            _secondsLeft.Value = s;
            yield return new WaitForSeconds(1f);
        }

        _secondsLeft.Value = -1;
        _frozen.Value = false;

        if (RaceTimer.Instance != null)
            RaceTimer.Instance.StartRace();
    }

    private void UpdateUI(int seconds)
    {
        bool active = seconds >= 0;
        countdownCanvas.SetActive(active);

        if (active)
            countdownText.text = seconds.ToString();
    }
}