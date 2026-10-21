using System;
using TMPro;
using UnityEngine;

public class TooltipPanel : MonoBehaviour
{
    [SerializeField] private GameElementManager manager;   // can stay empty, found at runtime
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    void OnEnable()
    {
        if (manager == null)
            manager = FindFirstObjectByType<GameElementManager>();

        if (manager == null)
        {
            Debug.LogError("TooltipPanel: no GameElementManager in the scene.", this);
            return;
        }

        manager.OnSelectionChanged += Show;
        Show(manager.SelectedElement);      // sync with the current state
    }

    void OnDisable()
    {
        if (manager != null) manager.OnSelectionChanged -= Show;
    }

    public void Show(GameElement element)
    {
        if (element == null) Clear();
        else Show(element.GetType());
    }

    public void Show(Type elementType) => ShowHover(elementType.Name);

    public void Clear()
    {
        nameText.text = "";
        descriptionText.text = "";
    }

    public void ShowHover(string typeName)
    {
        ElementText t = GameElementTexts.Get(typeName);
        nameText.text = t.name;
        descriptionText.text = t.description;
    }

    // Pointer left the button: back to the selected element, or empty
    public void EndHover() => Show(manager != null ? manager.SelectedElement : null);
}