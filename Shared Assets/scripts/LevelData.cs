using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int version = 1;
    public int nextId = 1;   // so that element id are always unique
    [SerializeReference] public List<GameElement> elements = new List<GameElement>();
}

// save: File.WriteAllText(path, JsonUtility.ToJson(level, true));
// load: level = JsonUtility.FromJson<LevelData>(File.ReadAllText(path));