using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelListPanel : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private Button rowPrefab;
    [SerializeField] private TMP_Text emptyText;

    public System.Action<string> OnLevelSelected;    // full path of the file

    private string Folder => SharedPaths.Levels;

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        foreach (Transform child in content) Destroy(child.gameObject);

        string[] files = new string[0];

        if (Directory.Exists(Folder))
        {
            files = Directory.GetFiles(Folder, "*.json")
                             .OrderByDescending(File.GetLastWriteTime)
                             .ToArray();
        }
        else
        {
            Debug.LogWarning($"Levels folder not found: {Path.GetFullPath(Folder)}");
        }


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