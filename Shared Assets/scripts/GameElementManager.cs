using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class GameElementManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private ElementPrefabCatalog catalog;
    [SerializeField] private Transform levelRoot;
    [SerializeField] private Transform spawnAnchor;

    public int selectedGameElementId;

    public event Action<GameElement> OnSelectionChanged;
    public event Action<GameElement> OnElementChanged;

    private LevelData level = new LevelData();
    private readonly Dictionary<int, ElementView> views = new Dictionary<int, ElementView>();

    public GameElement SelectedElement { get; private set; }
    public List<GameElement> gameElementList => level.elements;

    // this should be the only function used in the platformer
    public void SaveToJSON()
    {
        //TODO File.WriteAllText(path, JsonUtility.ToJson(level, true));
    }

    public void LoadFromJSON()
    {
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
    }

    /*
    public void DeleteSelectedGameElement()
    {
        DeleteGameElement(selectedGameElementId);
    }
    */
    
    
    public void ToggleLockGameElement()
    {
        if (!views.TryGetValue(selectedGameElementId, out var view) || view == null)
            return;

        view.IsLocked = !view.IsLocked;

        if (view.TryGetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(out var grab))
        {
            grab.trackPosition = view.IsLocked;
            grab.trackRotation = view.IsLocked;
            grab.trackScale = view.IsLocked;
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