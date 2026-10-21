using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class GameElementManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private ElementPrefabCatalog catalog;
    [SerializeField] private Transform levelRoot;
    [SerializeField] private Transform spawnAnchor;
    
    private string SaveFolder => SharedPaths.Levels;

    public int selectedGameElementId;

    public event Action<GameElement> OnSelectionChanged;
    public event Action<GameElement> OnElementChanged;

    private LevelData level = new LevelData();
    private readonly Dictionary<int, ElementView> views = new Dictionary<int, ElementView>();

    public GameElement SelectedElement { get; private set; }
    public List<GameElement> gameElementList => level.elements;
    
    public event Action<int> Grabbed;

    public void NotifyGrabbed(int elementId)
    {
        Grabbed?.Invoke(elementId);
    }

    
    public void SaveToJSON()
    {
        //TODO File.WriteAllText(path, JsonUtility.ToJson(level, true));
        
        LevelSaveData saveData = CreateSaveData(level);
        string json = JsonUtility.ToJson(saveData, true);
        
        string path = Path.Combine(
            SaveFolder,
            "test_name.json"
        );
        
        File.WriteAllText(path, json);
        Debug.Log($"Level saved to: {path}");
    }
    
    public LevelSaveData CreateSaveData(LevelData level)
    {
        LevelSaveData saveData = new LevelSaveData
        {
            version = level.version,
            nextId = level.nextId
        };

        foreach (GameElement element in level.elements)
        {
            saveData.elements.Add(new GameElementSaveData
            {
                id = element.id,
                name = element.Name,
                position = element.position,
                size = element.size,
                rotation = element.rotation
            });
        }

        return saveData;
    }

    public void LoadFromJSON()
    {
        //THIS FUNCTION SHOULD BE CALLED FROM A FILE IN SHARED ASSETS
        //TODO level = JsonUtility.FromJson<LevelData>(File.ReadAllText(path));
        //TODO then destroy existing views, instantiate one view per element under levelRoot, Bind + Apply.
        //TODO check for valid elements.
    }

    public void SelectGameElement(int id)
    {
        SelectedElement = level.elements.Find(e => e.id == id);
        selectedGameElementId = id;
        OnSelectionChanged?.Invoke(SelectedElement);   // null if the id wasn't found
    }

    public void ClearSelection()
    {
        SelectedElement = null;
        OnSelectionChanged?.Invoke(null);
    }

    public GameElement CreateGameElement(string typeName)
    {
        if (catalog == null || spawnAnchor == null || levelRoot == null)
        {
            Debug.LogError("GameElementManager: catalog, levelRoot or spawnAnchor is not assigned.", this);
            return null;
        }

        Type type = typeof(GameElement).Assembly.GetType(typeName);
        if (type == null || type.IsAbstract || !typeof(GameElement).IsAssignableFrom(type))
        {
            Debug.LogError($"Unknown element type '{typeName}'.", this);
            return null;
        }

        ElementView prefab = catalog.Get(typeName);
        if (prefab == null)
        {
            Debug.Log($"No prefab in the catalog for '{typeName}'.", this);
            return null;
        }

        var element = (GameElement)Activator.CreateInstance(type);
        element.id = level.nextId++;
        level.elements.Add(element);

        ElementView view = Instantiate(prefab, levelRoot);
        view.transform.SetPositionAndRotation(spawnAnchor.position, Quaternion.identity);
        view.Bind(element);
        view.WriteBack();

        views[element.id] = view;
        SelectGameElement(element.id);
        
        view.Grabbed += SelectGameElement;
        view.Grabbed += NotifyGrabbed //for the menu manager
            ;
        return element;
    }
    // Call after changing a value on an element (on the sliders...), refreshes its scene object
    public void NotifyChanged(GameElement element)
    {
        if (element != null && views.TryGetValue(element.id, out var view))
            view.Apply();

        OnElementChanged?.Invoke(element);
    }

    public void DeleteGameElement(int id)
    {
        if (views.TryGetValue(id, out var view))
        {
            if (view != null) Destroy(view.gameObject);
            views.Remove(id);
        }

        level.elements.RemoveAll(e => e.id == id);

        if (SelectedElement != null && SelectedElement.id == id)
            ClearSelection();
        
        view.Grabbed -= SelectGameElement;
    }

    /*
    public void DeleteSelectedGameElement()
    {
        DeleteGameElement(selectedGameElementId);
    }
    */
    
    
    public void ToggleLockGameElement()
    {
        Debug.Log("test1");
        if (!views.TryGetValue(selectedGameElementId, out var view) || view == null)
            return;
        Debug.Log("test2");
        Debug.Log(view.IsUnlocked);
        view.IsUnlocked = !view.IsUnlocked;

        if (view.TryGetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(out var grab))
        {
            Debug.Log($"test3 {view.IsUnlocked}");
            grab.trackPosition = view.IsUnlocked;
            grab.trackRotation = view.IsUnlocked;
            grab.trackScale = view.IsUnlocked;
            
            Debug.Log($"test4 {grab.trackPosition}");
        }
    }
    
    public void SwapMovingPlatform()
    {
        //TODO Implement
    }
    


    public void InteractionUtil(string buttonName)
    {
        if (buttonName == "lock")
        {
            ToggleLockGameElement();
        }
        else if (buttonName == "swap")
        {
            SwapMovingPlatform();
        }
        else if (buttonName == "delete")
        {
            Debug.Log($"deleting Game Element {selectedGameElementId}");
            DeleteGameElement(selectedGameElementId); //use DeleteSelectedGameElement if this is used elsewhere
        }
        else
        {
            Debug.LogError($" {buttonName} : Unknown button name");
        }
    }
    
    public ElementView GetView(int id) => views.TryGetValue(id, out var v) ? v : null;
}