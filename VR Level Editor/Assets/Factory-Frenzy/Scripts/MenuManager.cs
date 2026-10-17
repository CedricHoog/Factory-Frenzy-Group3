using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject elementMenuCanvas;
    [SerializeField] private GameObject saveMenuCanvas;
    [SerializeField] private GameObject loadMenuCanvas;

    [SerializeField] private GameObject confirmExitCanvas;
    [SerializeField] private GameObject confirmOverwriteCanvas;

    void Start() => ShowElementMenu();

    public void ShowElementMenu() => Show(elementMenuCanvas);
    public void ShowSaveMenu() => Show(saveMenuCanvas);
    public void ShowLoadMenu() => Show(loadMenuCanvas);

    public void ShowConfirmExitMenu() => Show(confirmExitCanvas);
    public void ShowConfirmOverwriteMenu() => Show(confirmOverwriteCanvas);

    private void Show(GameObject target)
    {
        elementMenuCanvas.SetActive(target == elementMenuCanvas);
        saveMenuCanvas.SetActive(target == saveMenuCanvas);
        loadMenuCanvas.SetActive(target == loadMenuCanvas);

        confirmExitCanvas.SetActive(target == confirmExitCanvas);
        confirmOverwriteCanvas.SetActive(target == confirmOverwriteCanvas);
    }
}