using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    private readonly List<int> _freePoints = new();
    private readonly Dictionary<ulong, int> _assigned = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
                _freePoints.Add(i);

            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        if (IsClient)
            StartCoroutine(AskForSpawnPoint());
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager != null)
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private IEnumerator AskForSpawnPoint()
    {
        while (NetworkManager.Singleton.LocalClient?.PlayerObject == null)
            yield return null;

        RequestSpawnPointServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnPointServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (_freePoints.Count == 0 || _assigned.ContainsKey(clientId))
            return;

        int random = Random.Range(0, _freePoints.Count);
        int pointIndex = _freePoints[random];
        _freePoints.RemoveAt(random);
        _assigned[clientId] = pointIndex;

        var onlyThisClient = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        MovePlayerClientRpc(pointIndex, onlyThisClient);
    }

    [ClientRpc]
    private void MovePlayerClientRpc(int pointIndex, ClientRpcParams rpcParams = default)
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject;
        var point = spawnPoints[pointIndex];

        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.GetComponent<NetworkTransform>()
              .Teleport(point.position, point.rotation, player.transform.localScale);

        if (controller != null) controller.enabled = true;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_assigned.TryGetValue(clientId, out int index))
        {
            _freePoints.Add(index);
            _assigned.Remove(clientId);
        }
    }
}