using System.Collections;
using System.Collections.Generic;
using FactoryFrenzy.Gameplay;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : NetworkBehaviour
{
    [SerializeField] private float waitingDuration = 3f;

    private readonly NetworkVariable<bool> _frozen = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        _frozen.OnValueChanged += OnFrozenChanged;
        PlayerMovementLock.SetMovementLocked(_frozen.Value);

        if (IsServer)
        {
            _frozen.Value = true;
            PlayerMovementLock.SetMovementLocked(true);
            NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }

    public override void OnNetworkDespawn()
    {
        _frozen.OnValueChanged -= OnFrozenChanged;
        PlayerMovementLock.SetMovementLocked(false);

        if (IsServer && NetworkManager != null && NetworkManager.SceneManager != null)
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    private void OnFrozenChanged(bool oldValue, bool newValue) =>
        PlayerMovementLock.SetMovementLocked(newValue);

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        StartCoroutine(UnfreezeAfterDelay());
    }

    private IEnumerator UnfreezeAfterDelay()
    {
        yield return new WaitForSeconds(waitingDuration);
        _frozen.Value = false;
    }
}