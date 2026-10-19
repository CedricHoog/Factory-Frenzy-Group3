using System;
using TMPro;
using UnityEngine;

public class TooltipPanel : MonoBehaviour
{
    [SerializeField] private GameElementManager manager;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    void OnEnable()
    {
        manager.OnSelectionChanged += Show;
        Show(manager.SelectedElement);      // sync with the current state
    }

    void OnDisable() => manager.OnSelectionChanged -= Show;

    public void Show(GameElement element)
    {
        if (element == null) Clear();
        else Show(element.GetType());
    }

    // For add mode: show a type's info before any element exists
    public void Show(Type elementType)
    {
        ElementText t = GameElementTexts.Get(elementType);
        nameText.text = t.name;
        descriptionText.text = t.description;
    }

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
    public void EndHover() => Show(manager.SelectedElement);
}