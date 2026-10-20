using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level/Element Prefab Catalog")]
public class ElementPrefabCatalog : ScriptableObject
{
    [Serializable] public struct Entry { public string typeName; public ElementView prefab; }
    [SerializeField] private List<Entry> entries;

    private Dictionary<string, ElementView> map;

    public ElementView Get(string typeName)
    {
        if (map == null)
        {
            map = new();
            foreach (var e in entries) map[e.typeName] = e.prefab;
        }
        return map.TryGetValue(typeName, out var p) ? p : null;
    }
}