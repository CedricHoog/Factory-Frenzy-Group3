using System.Collections.Generic;

[System.Serializable]
public class LevelSaveData
{
    public int version = 1;
    public int nextId = 1;
    public List<GameElementSaveData> elements = new();
}