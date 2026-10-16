using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace FactoryFrenzy.Gameplay
{
    public class PlayerData : NetworkBehaviour
    {
        public NetworkVariable<bool> IsReady = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public static readonly HashSet<PlayerData> All = new();
        public static event Action OnPlayersChanged;

        public override void OnNetworkSpawn()
        {
            All.Add(this);
            IsReady.OnValueChanged += OnReadyChanged;
            OnPlayersChanged?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            IsReady.OnValueChanged -= OnReadyChanged;
            All.Remove(this);
            OnPlayersChanged?.Invoke();
        }

        private void OnReadyChanged(bool oldValue, bool newValue) => OnPlayersChanged?.Invoke();

        [Rpc(SendTo.Server)]
        public void ToggleReadyRpc()
        {
            IsReady.Value = !IsReady.Value;
        }
    }
}