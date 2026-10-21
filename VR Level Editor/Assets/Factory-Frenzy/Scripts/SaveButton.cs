using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;



[RequireComponent(typeof(Button))]
public class SaveButton : MonoBehaviour
{
    private GameElementManager manager;
    private Button button;

    [SerializeField] private TMP_Text fileNameInput;
    

    void Start()
    {
        manager = FindFirstObjectByType<GameElementManager>();
        button = GetComponent<Button>();

        if (manager == null)
        {
            Debug.LogError("GameElementManager not found!");
            return;
        }
        
        if (fileNameInput == null)
        {
            Debug.LogError("fileNameInput not found!");
            return;
        }

        button.onClick.AddListener(SaveFile);
        
    }

    void SaveFile()
    {
        string fileName = fileNameInput.text.Replace("\u200B", "").Trim();
        manager.SaveToJSON(fileName);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(SaveFile);
    }
}