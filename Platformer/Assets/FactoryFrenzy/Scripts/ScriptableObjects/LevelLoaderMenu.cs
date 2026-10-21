using System.IO;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#else
using SFB; // UnityStandaloneFileBrowser (builds uniquement)
#endif

public class LevelLoaderMenu : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject menuCanvas;       // LoadMenuCanvas

    [Header("Fichier s�lectionn�")]
    [SerializeField] private TMP_Text fileNameText;       // FileNameText

    [Header("Liste des fichiers")]
    [SerializeField] private Transform Content;       // Scroll View > Viewport > Content
    [SerializeField] private GameObject emptyText;        // EmptyText
    [SerializeField] private Button levelButtonPrefab;    // un bouton avec un TMP en enfant

    [Header("Boutons")]
    [SerializeField] private Button loadFileButton;       // LoadFile Button
    [SerializeField] private Button cancelButton;         // CancelLoad Button

    [Header("Dossier des niveaux (relatif � la racine du d�p�t)")]
    [SerializeField] private string repoRelativeFolder = "Shared Assets/levels";

    private string _selectedPath = "";

    private static bool IsHost =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

    private string FolderPath
    {
        get
        {
#if UNITY_EDITOR
            string repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            return Path.GetFullPath(Path.Combine(repoRoot, repoRelativeFolder));
#else
        return Path.Combine(Application.persistentDataPath, "Levels");
#endif
        }
    }
    private void Awake()
    {
        loadFileButton.onClick.AddListener(OnLoadFileClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnDestroy()
    {
        loadFileButton.onClick.RemoveListener(OnLoadFileClicked);
        cancelButton.onClick.RemoveListener(OnCancelClicked);
    }

    private void Start() => SetMenuOpen(false);

    private void Update()
    {
        if (!IsHost) return; // seul l'h�te peut ouvrir le menu

        if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
            SetMenuOpen(!menuCanvas.activeSelf);
    }

    // ---------- Ouverture / fermeture ----------

    private void SetMenuOpen(bool open)
    {
        if (open && !IsHost) return;

        menuCanvas.SetActive(open);
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;

        if (open) RefreshList();
    }

    // ---------- Les deux boutons ----------

    private void OnLoadFileClicked()
    {
        if (!IsHost) return;
        if (string.IsNullOrEmpty(_selectedPath)) return;   // rien de s�lectionn�

        string json = File.ReadAllText(_selectedPath);

        if (!LevelParser.TryParse(json, out var entries, out string error))
        {
            Debug.LogError($"JSON invalide : {error}");
            fileNameText.text = "Fichier invalide";
            return;
        }

        LevelSelection.Set(Path.GetFileName(_selectedPath), json);
        Debug.Log($"Niveau charg� : {LevelSelection.FileName} ({entries.Count} �l�ments)");
        SetMenuOpen(false);
    }

    private void OnCancelClicked()
    {
        _selectedPath = "";
        UpdateFileNameText();
        SetMenuOpen(false);
    }

    // ---------- S�lection ----------

    private void SelectFile(string path)
    {
        if (!IsHost) return;

        if (Path.GetExtension(path).ToLower() != ".json")
        {
            return;
        }

        _selectedPath = path;
        UpdateFileNameText();
    }

    private void UpdateFileNameText()
    {
        fileNameText.text = string.IsNullOrEmpty(_selectedPath)
            ? "Aucun fichier s�lectionn�"
            : Path.GetFileName(_selectedPath);
    }

    // ---------- Liste ----------

    private void RefreshList()
    {
        UpdateFileNameText();

        foreach (Transform child in Content)
            Destroy(child.gameObject);

        Directory.CreateDirectory(FolderPath);
        string[] files = Directory.GetFiles(FolderPath, "*.json");

        emptyText.SetActive(files.Length == 0);

        foreach (string file in files)
        {
            string captured = file;
            Button b = Instantiate(levelButtonPrefab, Content);
            b.GetComponentInChildren<TMP_Text>().text = Path.GetFileName(file);
            b.onClick.AddListener(() => SelectFile(captured));
        }
    }
}