using FactoryFrenzy.Gameplay;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class FinishLine : NetworkBehaviour
{
    [SerializeField] private GameObject countdownCanvas;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int duration = 10;
    [SerializeField] private GameObject scoreboardPanel;
    [SerializeField] private TMP_Text scoreboardText;

    private readonly NetworkVariable<int> _secondsLeft = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private bool _hasSomeoneEnded;

    public override void OnNetworkSpawn()
    {
        _secondsLeft.OnValueChanged += OnSecondsChanged;
        UpdateUI(_secondsLeft.Value);
    }

    public override void OnNetworkDespawn()
    {
        _secondsLeft.OnValueChanged -= OnSecondsChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerData>();
        if (player == null || !player.IsOwner) return;

        PlayerFinishedRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayerFinishedRpc(RpcParams rpcParams = default)
    {
        if (!IsServer) return;
        if (RaceTimer.Instance == null || !RaceTimer.Instance.IsRunning) return;

        ulong senderId = rpcParams.Receive.SenderClientId;

        PlayerData player = null;
        foreach (var p in PlayerData.All)
        {
            if (p.OwnerClientId == senderId) { player = p; break; }
        }
        if (player == null) return;

        if (player.FinishTime.Value != PlayerData.Running) return;

        player.FinishTime.Value = RaceTimer.Instance.Elapsed;

        if (!_hasSomeoneEnded)
        {
            _hasSomeoneEnded = true;
            StartCoroutine(CountdownRoutine());
        }
    }

    private IEnumerator CountdownRoutine()
    {
        for (int s = duration; s >= 0; s--)
        {
            _secondsLeft.Value = s;
            yield return new WaitForSeconds(1f);
        }

        _secondsLeft.Value = -1;

        foreach (var p in PlayerData.All)
            if (p.FinishTime.Value == PlayerData.Running)
                p.FinishTime.Value = PlayerData.NoTime;

        ShowScoreboardRpc();
    }

    private void OnSecondsChanged(int oldValue, int newValue) => UpdateUI(newValue);

    private void UpdateUI(int seconds)
    {
        bool active = seconds >= 0;
        countdownCanvas.SetActive(active);
        if (active) countdownText.text = seconds.ToString();
    }

    [Rpc(SendTo.Everyone)]
    private void ShowScoreboardRpc()
    {
        PlayerMovementLock.SetMovementLocked(true);
        scoreboardPanel.SetActive(true);

        var players = new List<PlayerData>(PlayerData.All);

        players.Sort((a, b) =>
        {
            float ta = a.FinishTime.Value, tb = b.FinishTime.Value;
            bool aOut = ta < 0f, bOut = tb < 0f;
            if (aOut && bOut) return 0;
            if (aOut) return 1;
            if (bOut) return -1;
            return ta.CompareTo(tb);
        });

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>Classement</b>");
        sb.AppendLine("------------------------------------\n");

        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            string pseudo = p.PlayerPseudo.Value.ToString();
            if (pseudo == "") pseudo = $"Joueur {p.OwnerClientId}";

            float t = p.FinishTime.Value;
            string time = t >= 0f ? t.ToString("0.00") + " s" : "--";

            string color = t < 0f ? "#888888"
                         : i == 0 ? "#FFD700"
                         : i == 1 ? "#C0C0C0"
                         : i == 2 ? "#CD7F32"
                         : "#FFFFFF";

            sb.AppendLine($"<color={color}>{i + 1}. {pseudo} : {time}</color>");
        }

        scoreboardText.text = sb.ToString();
    }
}