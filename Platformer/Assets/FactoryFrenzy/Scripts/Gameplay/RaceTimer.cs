using FactoryFrenzy.Gameplay;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class RaceTimer : NetworkBehaviour
{
    public static RaceTimer Instance { get; private set; }

    //[SerializeField] private TMP_Text timerText;

    private readonly NetworkVariable<double> _startTime = new(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public bool IsRunning => _startTime.Value >= 0;
    public float Elapsed => IsRunning ? (float)(NetworkManager.ServerTime.Time - _startTime.Value) : 0f;

    private PlayerData _localPlayer;

    public override void OnNetworkSpawn() => Instance = this;
    public override void OnNetworkDespawn() => Instance = null;

    public void StartRace()
    {
        if (!IsServer) return;

        foreach (var p in PlayerData.All)
            p.FinishTime.Value = PlayerData.Running;

        _startTime.Value = NetworkManager.ServerTime.Time;
    }

    private void Update()
    {
        if (!IsRunning) return;

        if (_localPlayer == null)
        {
            var obj = NetworkManager.LocalClient?.PlayerObject;
            if (obj == null) return;
            _localPlayer = obj.GetComponent<PlayerData>();
        }

        //float finish = _localPlayer.FinishTime.Value;

        //if (finish == PlayerData.NoTime) timerText.text = "--";
        //else if (finish >= 0f) timerText.text = finish.ToString("0.00");
        //else timerText.text = Elapsed.ToString("0.00");
    }
}