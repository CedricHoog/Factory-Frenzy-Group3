using System.Collections;
using FactoryFrenzy.Gameplay;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyCountdown : NetworkBehaviour
{
    [SerializeField] private GameObject countdownCanvas;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int duration = 5;

    private readonly NetworkVariable<int> _secondsLeft = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private Coroutine _countdown;

    public override void OnNetworkSpawn()
    {
        _secondsLeft.OnValueChanged += OnSecondsChanged;
        UpdateUI(_secondsLeft.Value);

        if (IsServer)
        {
            PlayerData.OnPlayersChanged += CheckAllReady;
            CheckAllReady();
        }
    }

    public override void OnNetworkDespawn()
    {
        _secondsLeft.OnValueChanged -= OnSecondsChanged;

        if (IsServer)
            PlayerData.OnPlayersChanged -= CheckAllReady;
    }

    private void CheckAllReady()
    {
        int total = PlayerData.All.Count;
        int ready = 0;
        foreach (var p in PlayerData.All)
            if (p.IsReady.Value) ready++;

        bool allReady = total > 0 && ready == total;

        if (allReady && _countdown == null)
        {
            _countdown = StartCoroutine(CountdownRoutine());
        }
        else if (!allReady && _countdown != null)
        {
            StopCoroutine(_countdown);
            _countdown = null;
            _secondsLeft.Value = -1;
        }
    }

    private IEnumerator CountdownRoutine()
    {
        for (int s = duration; s > 0; s--)
        {
            _secondsLeft.Value = s;
            yield return new WaitForSeconds(1f);
        }

        _secondsLeft.Value = 0;
        GameState.GameStarted = true;
        NetworkManager.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    private void OnSecondsChanged(int oldValue, int newValue) => UpdateUI(newValue);

    private void UpdateUI(int seconds)
    {
        bool active = seconds >= 0;
        countdownCanvas.SetActive(active);

        if (active)
            countdownText.text = seconds.ToString();
    }
}