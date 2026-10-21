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

    private TMP_Text fileNameText;

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
        
    }

    void SaveFile()
    {
        GameObject obj = FindIncludingInactive("SaveName");
        fileNameText = obj.GetComponent<TMP_Text>();
        string fileName = fileNameText.text.Trim();
        manager.SaveToJSON(fileName);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(SaveFile);
    }
    //FIXME THIS IS HORRIBLE. JUST FIND A WAY TO LINK THE TEXT (text gets unlinked)
    GameObject FindIncludingInactive(string objectName)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(go => go.name == objectName
                && go.scene.IsValid());
    }
}