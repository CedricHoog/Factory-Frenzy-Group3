using System;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Globalization;


public class ConfirmSave : MonoBehaviour
{
    [SerializeField] private TMP_Text input;
    [SerializeField] private TMP_Text statusLabel;

    [SerializeField] private string newText = "New level";
    [SerializeField] private string overwriteText = "Will overwrite existing level";
    [SerializeField] private string emptyText = "";

    [SerializeField] private string folder;
    private string lastRaw;

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

        statusLabel.text = string.IsNullOrEmpty(name) ? emptyText
            : Exists(name)                            ? overwriteText
            :                                           newText;
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