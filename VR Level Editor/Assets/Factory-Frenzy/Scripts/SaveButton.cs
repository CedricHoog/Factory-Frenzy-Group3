using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



[RequireComponent(typeof(Button))]
public class SaveButton : MonoBehaviour
{
    private GameElementManager manager;
    private Button button;

    void Start()
    {
        manager = FindFirstObjectByType<GameElementManager>();
        button = GetComponent<Button>();

        if (manager == null)
        {
            Debug.LogError("GameElementManager not found!");
            return;
        }

        button.onClick.AddListener(manager.SaveToJSON);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(manager.SaveToJSON);
    }
}