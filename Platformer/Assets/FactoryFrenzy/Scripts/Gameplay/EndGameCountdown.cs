using FactoryFrenzy.Gameplay;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class EndGameCountdown : NetworkBehaviour
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

    private bool _hasSomeoneEnded; // utilisé côté serveur uniquement

    public override void OnNetworkSpawn()
    {
        _secondsLeft.OnValueChanged += OnSecondsChanged;
        UpdateUI(_secondsLeft.Value);
    }

    public override void OnNetworkDespawn()
    {
        _secondsLeft.OnValueChanged -= OnSecondsChanged;
    }

    // Exécuté sur chaque machine : on ne réagit que pour le joueur local
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<PlayerData>();
        if (player == null || !player.IsOwner) return;

        PlayerFinishedRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayerFinishedRpc()
    {
        if (_hasSomeoneEnded) return;
        _hasSomeoneEnded = true;
        StartCoroutine(CountdownRoutine());
    }

    // Serveur uniquement
    private IEnumerator CountdownRoutine()
    {
        for (int s = duration; s >= 0; s--)
        {
            _secondsLeft.Value = s;
            yield return new WaitForSeconds(1f);
        }

        _secondsLeft.Value = -1;
        ShowScoreboardRpc();
    }

    // Tous les clients
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
        scoreboardPanel.SetActive(true);

        //var players = FindObjectsByType<PlayerData>(FindObjectsSortMode.None);

        //System.Array.Sort(players, (a, b) => b.PlayerScore.Value.CompareTo(a.PlayerScore.Value));

        //var sb = new System.Text.StringBuilder();
        //sb.AppendLine("<b>Classement</b>");
        //sb.AppendLine("------------------------------------\n");

        //int rank = 1;
        //for (int i = 0; i < players.Length; i++)
        //{
        //    if (i > 0 && players[i].PlayerScore.Value < players[i - 1].PlayerScore.Value)
        //        rank = i + 1;

        //    string pseudo = players[i].PlayerPseudo.Value.ToString();
        //    if (pseudo == "") pseudo = $"Joueur {players[i].OwnerClientId}";

        //    string color = rank == 1 ? "#FFD700"
        //                 : rank == 2 ? "#C0C0C0"
        //                 : rank == 3 ? "#CD7F32"
        //                 : "#FFFFFF";

        //    sb.AppendLine($"<color={color}>{rank}. {pseudo} : {players[i].PlayerScore.Value}</color>");
        //}

        //scoreboardText.text = sb.ToString();
    }
}