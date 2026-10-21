using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;



[RequireComponent(typeof(Button))]
public class SaveButton : MonoBehaviour
{
    private GameElementManager manager;
    private Button button;

    [SerializeField] private TMP_Text saveName;

    void Start()
    {
        manager = FindFirstObjectByType<GameElementManager>();
        button = GetComponent<Button>();

        if (manager == null)
        {
            Debug.LogError("GameElementManager not found!");
            return;
        }

        button.onClick.AddListener(SaveFile);

        if (saveName == null)
        {
            Debug.LogError("Filename TMP_Text is not assigned!");
            return;
        }
    }

    void SaveFile()
    {
        string fileName = saveName.text.Trim();
        manager.SaveToJSON(fileName);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(SaveFile);
    }
}