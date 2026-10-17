using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject elementMenuCanvas;
    [SerializeField] private GameObject saveMenuCanvas;
    [SerializeField] private GameObject loadMenuCanvas;

    void Start() => ShowElementMenu();

    // Public so the buttons' OnClick can see them
    public void ShowElementMenu() => Show(elementMenuCanvas);
    public void ShowSaveMenu() => Show(saveMenuCanvas);
    public void ShowLoadMenu() => Show(loadMenuCanvas);

    // The only place where menus are toggled, so exactly one is active
    private void Show(GameObject target)
    {
        Debug.Log("show function called");
        elementMenuCanvas.SetActive(target == elementMenuCanvas);
        saveMenuCanvas.SetActive(target == saveMenuCanvas);
        loadMenuCanvas.SetActive(target == loadMenuCanvas);
    }
}