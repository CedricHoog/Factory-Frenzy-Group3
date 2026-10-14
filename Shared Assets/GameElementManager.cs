using System;
using System.Collections.Generic;
using UnityEngine;

/* TODO
[Serializable]
public class LevelData
{
    public int version = 1;
    public int nextId = 1;
    [SerializeReference] public List<GameElement> elements = new();
}

// save
File.WriteAllText(path, JsonUtility.ToJson(level, true));
// load
level = JsonUtility.FromJson<LevelData>(File.ReadAllText(path));
*/

public class GameElementManager : MonoBehaviour
{
    public List<GameElement> gameElementList = new List<GameElement>();
    public int selectedGameElementId;
    
    /*
    In Unity, as an Inspector field:

    If the subclasses are MonoBehaviours, use [SerializeField] private List<GameElements> items; and drag objects in.
    If they’re plain C# classes, add [SerializeReference] instead of [SerializeField], and note that the default Inspector can’t pick the subclass type.
    */

    //this should be the only function used in the platformer
    public void SaveToJSON()
    {
        //TODO create a json file based on gameElementList
    }

    public void LoadFromJSON()
    {
        //TODO fill gameElementList with the json file. Might need to add check for valid elements.
    }

    public void SelectGameElement()
    {
    //TODO : need a function made to get an gameElement an put it in selectedGameElementId.
    //(also set selectedGameElementName & selectedGameElementDescription for the tooltip)
    //When an item is selected, witch the UI from add element mode to modify element mode.
    //Some sections could be added/remove based on the name of the game element
    }

    public void ToggleUIMode()
    {
    //might need to be in a different file ? idk
    //TODO : toggle add mode / modify mode
    //also check selectedGameElementId for some values
    }

    public void CreateGameElement()
    {
        //TODO create a game element based on what is clicked in the UI. also think about where to position it. Probably not 0,0,0.
        //id needs to be unique
    }

    public void DeleteGameElement(int id)
    {
        //TODO delete a game element based on it's ID
    }
}