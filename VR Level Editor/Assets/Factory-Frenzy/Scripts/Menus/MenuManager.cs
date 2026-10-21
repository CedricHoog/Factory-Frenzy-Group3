using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject elementMenuCanvas;
    [SerializeField] private GameObject saveMenuCanvas;
    [SerializeField] private GameObject loadMenuCanvas;

    [SerializeField] private GameObject confirmExitCanvas;
    [SerializeField] private GameObject confirmOverwriteCanvas;
    
    [SerializeField] private GameObject addElementPanel;
    [SerializeField] private GameObject modifyElementPanel;
    
    [SerializeField] private Button swapButton;

    private GameObject currentCanvas;
    
    private GameElementManager gameElementManager;

    void Start()
    {
        ShowElementMenu();
        ShowAddElementPanel();
        DeactivateSwapButton();
    }
    
    public void ShowElementMenu() => ShowCanvas(elementMenuCanvas);
    public void ShowSaveMenu() => ShowCanvas(saveMenuCanvas);
    public void ShowLoadMenu() => ShowCanvas(loadMenuCanvas);

    public void ShowConfirmExitMenu() => ShowCanvas(confirmExitCanvas);
    public void ShowConfirmOverwriteMenu() => ShowCanvas(confirmOverwriteCanvas);

    public void ShowAddElementPanel() => ShowPanel(true);
    public void ShowModifyElementPanel() => ShowPanel(false);
    
    public void ActivateSwapButton() => SetInteractableSwapButton(true);
    public void DeactivateSwapButton() => SetInteractableSwapButton(false);
    
    private void Awake()
    {
        gameElementManager = FindFirstObjectByType<GameElementManager>();
        gameElementManager.Grabbed += OnItemGrabbed;
    }
    
    private void OnItemGrabbed(int elementId)
    {
        ShowModifyElementPanel();
    }
    

    private void ShowCanvas(GameObject target)
    {
        elementMenuCanvas.SetActive(target == elementMenuCanvas);
        saveMenuCanvas.SetActive(target == saveMenuCanvas);
        loadMenuCanvas.SetActive(target == loadMenuCanvas);

        confirmExitCanvas.SetActive(target == confirmExitCanvas);
        confirmOverwriteCanvas.SetActive(target == confirmOverwriteCanvas);

        currentCanvas = target;
    }
    
    private void ShowPanel(bool addMode)
    {
        if (currentCanvas != elementMenuCanvas) return;
        if (addMode) SetInteractableSwapButton(false);
        addElementPanel.SetActive(addMode);
        modifyElementPanel.SetActive(!addMode);

    }

    private void SetInteractableSwapButton(bool activated)
    {
        swapButton.interactable = activated;
    }
}