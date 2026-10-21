using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelBuilder : NetworkBehaviour
{
    [Serializable]
    public struct PrefabEntry
    {
        public string name;            // identique au champ "Name" du JSON
        public NetworkObject prefab;
    }

    [SerializeField] private PrefabEntry[] catalog;

    private readonly List<NetworkObject> _spawned = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;     // seul le serveur construit
        Build();
    }

    private void Build()
    {
        if (!LevelSelection.HasLevel)
        {
            Debug.Log("Aucun niveau sélectionné : niveau par défaut conservé.");
            return;
        }

        if (!LevelParser.TryParse(LevelSelection.Json, out var entries, out string error))
        {
            Debug.LogError($"Niveau invalide : {error}");
            return;
        }

        var map = new Dictionary<string, NetworkObject>();
        foreach (var p in catalog) map[p.name] = p.prefab;

        foreach (var e in entries)
        {
            if (!map.TryGetValue(e.Name, out var prefab) || prefab == null)
            {
                Debug.LogWarning($"Aucun prefab pour « {e.Name} » (Id {e.Id}), ignoré.");
                continue;
            }

            NetworkObject obj = Instantiate(prefab, e.Position, e.Rotation);
            if (e.HasSize) obj.transform.localScale = e.Size;

            obj.Spawn();
            _spawned.Add(obj);
        }

        Debug.Log($"Niveau « {LevelSelection.FileName} » construit : {_spawned.Count} objets.");
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        foreach (var o in _spawned)
            if (o != null && o.IsSpawned) o.Despawn(true);
        _spawned.Clear();
    }
}