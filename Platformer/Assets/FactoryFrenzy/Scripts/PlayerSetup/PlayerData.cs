using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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

        [SerializeField] private TMP_Text pseudoText;

        public NetworkVariable<FixedString32Bytes> PlayerPseudo = new(
           default,
           NetworkVariableReadPermission.Everyone,
           NetworkVariableWritePermission.Owner);

        public const float Running = -1f;
        public const float NoTime = -2f;

        public NetworkVariable<float> FinishTime = new(
            Running,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            All.Add(this);
            IsReady.OnValueChanged += OnReadyChanged;
            PlayerPseudo.OnValueChanged += OnPseudoChanged;

            pseudoText.text = PlayerPseudo.Value.ToString();
            if (IsOwner) pseudoText.gameObject.SetActive(false); // on ne voit pas son propre pseudo

            OnPlayersChanged?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            IsReady.OnValueChanged -= OnReadyChanged;
            PlayerPseudo.OnValueChanged -= OnPseudoChanged;
            All.Remove(this);
            OnPlayersChanged?.Invoke();
        }
        private void OnReadyChanged(bool oldValue, bool newValue) => OnPlayersChanged?.Invoke();

        private void OnPseudoChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue) =>
            pseudoText.text = newValue.ToString();


        [Rpc(SendTo.Server)]
        public void ToggleReadyRpc()
        {
            IsReady.Value = !IsReady.Value;
        }
    }
}