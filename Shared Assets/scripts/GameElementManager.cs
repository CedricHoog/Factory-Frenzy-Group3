using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

// Editor project. Owns the level data and the scene objects that represent it.
public class GameElementManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private ElementPrefabCatalog catalog;   // type name -> prefab
    [SerializeField] private Transform levelRoot;            // parent of placed elements (keep at origin, scale 1)
    [SerializeField] private Transform spawnAnchor;          // child of the VR controller, in front of the hand
    
    [Header("Input")]
    [SerializeField] private InputActionProperty placeAction;

    public int selectedGameElementId;

    // ---------- Events ----------
    public event Action<GameElement> OnSelectionChanged;     // null when nothing is selected
    public event Action<GameElement> OnElementChanged;       // size / rotation / position was modified

    // ---------- State ----------
    private LevelData level = new LevelData();
    private readonly Dictionary<int, ElementView> views = new Dictionary<int, ElementView>();
    private ElementView held;                                // created but not placed yet

    public GameElement SelectedElement { get; private set; }
    public List<GameElement> gameElementList => level.elements;   // kept for existing scripts
    public bool HasHeldElement => held != null;
    
    private void OnEnable()
    {
        placeAction.action.Enable();
        placeAction.action.performed += OnPlacePressed;
    }

    private void OnDisable()
    {
        placeAction.action.performed -= OnPlacePressed;
    }

    private void OnPlacePressed(InputAction.CallbackContext ctx) => PlaceHeldElement();

    // ---------- Save / Load ----------
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

    // ---------- Selection ----------
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

    // ---------- Creation ----------
    // Called by the 9 buttons with the class name, e.g. "PlatformMoving"
    public GameElement CreateGameElement(string typeName)
    {
        if (catalog == null || spawnAnchor == null || levelRoot == null)
        {
            Debug.LogError("GameElementManager: catalog, levelRoot or spawnAnchor is not assigned.", this);
            return null;
        }

        // The classes live in the shared assembly, so look the type up there
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

        // Policy: an element that was never placed is replaced by the new one
        if (held != null) DeleteGameElement(held.Element.id);

        // 1. data
        var element = (GameElement)Activator.CreateInstance(type);
        element.id = level.nextId++;
        level.elements.Add(element);

        // 2. scene object, child of the controller anchor
        ElementView view = Instantiate(prefab, spawnAnchor);
        view.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        view.Bind(element);

        // 3. bookkeeping
        views[element.id] = view;
        held = view;
        SelectGameElement(element.id);

        return element;
    }

    // Called by the "place" input action (trigger / grip)
    public void PlaceHeldElement()
    {
        if (held == null) return;

        held.transform.SetParent(levelRoot, true);   // keep the world position
        held.WriteBack();                            // transform -> element.position / rotation

        GameElement placed = held.Element;
        held = null;
        OnElementChanged?.Invoke(placed);
    }

    // ---------- Modification ----------
    // Call after changing a value on an element (sliders...): refreshes its scene object
    public void NotifyChanged(GameElement element)
    {
        if (element != null && views.TryGetValue(element.id, out var view))
            view.Apply();
        OnElementChanged?.Invoke(element);
    }

    // ---------- Deletion ----------
    public void DeleteGameElement(int id)
    {
        if (views.TryGetValue(id, out var view))
        {
            if (view != null) Destroy(view.gameObject);
            views.Remove(id);
        }

        level.elements.RemoveAll(e => e.id == id);

        if (held != null && held.Element.id == id) held = null;
        if (SelectedElement != null && SelectedElement.id == id) ClearSelection();
    }

    // ---------- Lookup ----------
    public ElementView GetView(int id) => views.TryGetValue(id, out var v) ? v : null;
}