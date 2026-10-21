using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;


public class ConfirmSave : MonoBehaviour
{
    [SerializeField] private TMP_Text input;
    [SerializeField] private TMP_Text statusLabel;
    
    [SerializeField] private string invalidText = "Your level needs EXACTLY 1 starting platform and 1 ending platform.";
    [SerializeField] private string newText = "Confirm save.";
    [SerializeField] private string overwriteText = "A file already has that name. Overwrite existing level ?";
    [SerializeField] private string emptyText = "Your file has no name.";

    [SerializeField] private string folder;
    [SerializeField] private Button saveButton;
    private string lastRaw;
    
    public GameElementManager manager;

    void Awake()
    {
        folder = SharedPaths.Levels;
    }

    void OnEnable()
    {
        lastRaw = null;
    }

    void Update()
    {
        if (input == null || statusLabel == null) return;

        string raw = input.GetParsedText();;
        if (raw == lastRaw) return;

        lastRaw = raw;
        UpdateLabel(raw);
    }
    
    public void ForceRefresh()
    {
        lastRaw = null;
    }

    public void UpdateLabel(string raw)
    {
        string name = Normalize(raw);
        
        if (manager == null) manager = FindFirstObjectByType<GameElementManager>();
        bool levelValid = manager != null && manager.HasValidStartAndEnd;

        if (!levelValid)
        {
            statusLabel.text = invalidText;
        }
        
        else
        {
            
        statusLabel.text = string.IsNullOrEmpty(name) ? emptyText
            : Exists(name)                            ? overwriteText
            :                                           newText;
        }
        saveButton.interactable = levelValid && !string.IsNullOrEmpty(name);
    }

    string Normalize(string raw)
    {
        if (raw == null) return "";

        // Drop control characters and invisible "format" characters (zero-width space, BOM, etc.)
        var sb = new System.Text.StringBuilder(raw.Length);
        foreach (char c in raw)
        {
            if (char.IsControl(c)) continue;
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format) continue;
            sb.Append(c);
        }

        string name = sb.ToString().Trim();

        if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            name = name.Substring(0, name.Length - 5).TrimEnd();

        return name;
    }

    bool Exists(string name)
    {
        return File.Exists(Path.Combine(folder, name + ".json"));
    }   
}