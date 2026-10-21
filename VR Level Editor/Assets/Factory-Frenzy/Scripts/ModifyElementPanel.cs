using UnityEngine;

// On the modify panel. Loads the selected element's size / rotation into 6 sliders,
// and writes slider moves back to the element in real time.
public class ModifyElementPanel : MonoBehaviour
{
    [SerializeField] private GameElementManager manager;   // found at runtime if empty (a prefab can't reference scene objects)

    [Header("Size")]
    [SerializeField] private ModifyElementSlider sizeX;
    [SerializeField] private ModifyElementSlider sizeY;
    [SerializeField] private ModifyElementSlider sizeZ;

    [Header("Rotation")]
    [SerializeField] private ModifyElementSlider rotationX;
    [SerializeField] private ModifyElementSlider rotationY;
    [SerializeField] private ModifyElementSlider rotationZ;

    private GameElement current;

    void OnEnable()
    {
        if (manager == null) manager = FindFirstObjectByType<GameElementManager>();
        if (manager == null)
        {
            Debug.LogError("ModifyElementPanel: no GameElementManager in the scene.", this);
            return;
        }

        manager.OnSelectionChanged += Load;
        manager.OnElementChanged += OnElementChanged;

        sizeX.ValueChanged += OnSizeMoved;
        sizeY.ValueChanged += OnSizeMoved;
        sizeZ.ValueChanged += OnSizeMoved;
        rotationX.ValueChanged += OnRotationMoved;
        rotationY.ValueChanged += OnRotationMoved;
        rotationZ.ValueChanged += OnRotationMoved;

        Load(manager.SelectedElement);   // the panel may open after the selection happened
    }

    void OnDisable()
    {
        if (manager != null)
        {
            manager.OnSelectionChanged -= Load;
            manager.OnElementChanged -= OnElementChanged;
        }

        sizeX.ValueChanged -= OnSizeMoved;
        sizeY.ValueChanged -= OnSizeMoved;
        sizeZ.ValueChanged -= OnSizeMoved;
        rotationX.ValueChanged -= OnRotationMoved;
        rotationY.ValueChanged -= OnRotationMoved;
        rotationZ.ValueChanged -= OnRotationMoved;
    }

    // element -> sliders (silent: no write-back loop)
    private void Load(GameElement element)
    {
        current = element;
        if (element == null) return;

        sizeX.SetValueWithoutNotify(element.size.x);
        sizeY.SetValueWithoutNotify(element.size.y);
        sizeZ.SetValueWithoutNotify(element.size.z);

        rotationX.SetValueWithoutNotify(element.rotation.x);
        rotationY.SetValueWithoutNotify(element.rotation.y);
        rotationZ.SetValueWithoutNotify(element.rotation.z);
    }

    // the selected element changed from elsewhere (placed, moved...): refresh the sliders
    private void OnElementChanged(GameElement element)
    {
        if (element == current) Load(element);
    }

    // sliders -> element (real time)
    private void OnSizeMoved(float _)
    {
        if (current == null) return;
        current.size = new Vector3(sizeX.Value, sizeY.Value, sizeZ.Value);
        manager.NotifyChanged(current);
    }

    private void OnRotationMoved(float _)
    {
        if (current == null) return;
        current.rotation = new Vector3(rotationX.Value, rotationY.Value, rotationZ.Value);
        manager.NotifyChanged(current);
    }
}