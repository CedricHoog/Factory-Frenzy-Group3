using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelListPanel : MonoBehaviour
{
    [SerializeField] private Transform content;      // the ScrollView's Content
    [SerializeField] private Button rowPrefab;
    [SerializeField] private TMP_Text emptyText;

    public System.Action<string> OnLevelSelected;    // full path of the file

    //private string Folder => Path.Combine(Application.persistentDataPath, "levels"); //FIXME THIS SHOULD BE CLEANER IN A FULL RELEASE
    //TODO USE A LEVEL STORAGE SCRIPT IN SHARED ASSETS INSTEAD OF HARDCODING IT
    private string Folder => @"..\..\..\Packages\factory.frenzy.shared\levels";

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        Directory.CreateDirectory(Folder);
        var files = Directory.GetFiles(Folder, "*.json")
                             .OrderByDescending(File.GetLastWriteTime)   // newest first
                             .ToArray();

        if (emptyText != null) emptyText.gameObject.SetActive(files.Length == 0);

        foreach (string path in files)
        {
            string captured = path;   // avoid closure issues
            var row = Instantiate(rowPrefab, content);
            row.GetComponentInChildren<TMP_Text>().text = Path.GetFileNameWithoutExtension(captured);
            row.onClick.AddListener(() => OnLevelSelected?.Invoke(captured));
        }
    }
}