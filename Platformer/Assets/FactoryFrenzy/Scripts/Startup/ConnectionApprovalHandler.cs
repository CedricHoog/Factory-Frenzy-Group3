using Unity.Netcode;
using UnityEngine;
using static Unity.Netcode.NetworkManager;

public class ConnectionApprovalHandler : MonoBehaviour
{
    private NetworkManager m_NetworkManager;

    public int MaxNumberOfPlayers = 6;
    private int _numberOfPlayers = 0;

    private void Start()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        if (m_NetworkManager != null)
        {
            m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            m_NetworkManager.ConnectionApprovalCallback += CheckApprovalCallback;
        }

        if (MaxNumberOfPlayers <= 0)
            MaxNumberOfPlayers = 1;
    }

    private void OnDestroy()
    {
        if (m_NetworkManager != null)
        {
            m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            m_NetworkManager.ConnectionApprovalCallback -= CheckApprovalCallback;
        }
    }

    private void CheckApprovalCallback(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
    {
        bool isHost = request.ClientNetworkId == NetworkManager.ServerClientId;

        if (GameState.GameStarted && !isHost)
        {
            Refuse(response, "Game already launched.");
            return;
        }

        if (_numberOfPlayers >= MaxNumberOfPlayers)
        {
            Refuse(response, "Too many players in lobby!");
            return;
        }

        _numberOfPlayers++;
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = new Vector3(0, 3, 0);
    }

    private void Refuse(ConnectionApprovalResponse response, string reason)
    {
        response.Approved = false;
        response.CreatePlayerObject = false;
        response.Reason = reason;
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        _numberOfPlayers = Mathf.Max(0, _numberOfPlayers - 1);

        if (!m_NetworkManager.IsServer
            && m_NetworkManager.DisconnectReason != string.Empty
            && !m_NetworkManager.IsApproved)
        {
            Debug.Log($"Approval Declined Reason: {m_NetworkManager.DisconnectReason}");
        }
    }
}