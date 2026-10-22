using System;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelLoaderMenu : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private GameObject menuCanvas;       // LoadMenuCanvas

    [Header("Fichier s�lectionn�")]
    [SerializeField] private TMP_Text fileNameText;       // FileNameText

    [Header("Liste des fichiers")]
    [SerializeField] private Transform Content;           // Scroll View > Viewport > Content
    [SerializeField] private GameObject emptyText;        // EmptyText
    [SerializeField] private Button levelButtonPrefab;    // un bouton avec un TMP en enfant

    [Header("Boutons")]
    [SerializeField] private Button loadFileButton;       // LoadFile Button
    [SerializeField] private Button cancelButton;         // CancelLoad Button

    [Header("Dossier des niveaux (relatif � la racine du d�p�t)")]
    private string _selectedPath = "";
    private string _selectedFolderPath = "";

    private static bool IsHost =>
        NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

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

        if (open)
        {
            _selectedPath = "";
            if (TryChooseFolder(out _selectedFolderPath))
                RefreshList();
            else
            {
                ClearList();
                emptyText.SetActive(true);
                UpdateFileNameText();
            }
        }
    }

    // ---------- Les deux boutons ----------

    private void OnLoadFileClicked()
    {
        if (!IsHost) return;
        if (string.IsNullOrEmpty(_selectedPath)) return;   // rien de s�lectionn�

        string json;
        try
        {
            json = File.ReadAllText(_selectedPath);
        }
        catch (IOException exception)
        {
            Debug.LogError($"Could not read level file '{_selectedPath}': {exception.Message}", this);
            fileNameText.text = "Impossible de lire le fichier";
            return;
        }
        catch (UnauthorizedAccessException exception)
        {
            Debug.LogError($"Access denied to level file '{_selectedPath}': {exception.Message}", this);
            fileNameText.text = "Accès au fichier refusé";
            return;
        }

        if (!LevelParser.TryParse(json, out var entries, out string error))
        {
            Debug.LogError($"JSON invalide : {error}");
            fileNameText.text = "Fichier invalide";
            return;
        }

        LevelSelection.Set(Path.GetFileName(_selectedPath), json);
        Debug.Log($"Niveau charg� : {LevelSelection.FileName} ({entries.Count} �l�ments)");
        SetMenuOpen(false);
        if (string.IsNullOrEmpty(_selectedPath)) return;   // rien de s�lectionn�

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

        ClearList();

        string[] files;
        try
        {
            files = Directory.GetFiles(_selectedFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        }
        catch (IOException exception)
        {
            Debug.LogError($"Could not read JSON files from '{_selectedFolderPath}': {exception.Message}", this);
            emptyText.SetActive(true);
            return;
        }
        catch (UnauthorizedAccessException exception)
        {
            Debug.LogError($"Access denied to level folder '{_selectedFolderPath}': {exception.Message}", this);
            emptyText.SetActive(true);
            return;
        }

        emptyText.SetActive(files.Length == 0);

        foreach (string file in files)
        {
            string captured = file;
            Button b = Instantiate(levelButtonPrefab, Content);
            b.GetComponentInChildren<TMP_Text>().text = Path.GetFileName(file);
            b.onClick.AddListener(() => SelectFile(captured));
        }
    }

    private void ClearList()
    {
        foreach (Transform child in Content)
            Destroy(child.gameObject);
    }

    private bool TryChooseFolder(out string folderPath)
    {
        folderPath = "";
#if UNITY_EDITOR
        folderPath = EditorUtility.OpenFolderPanel(
            "Choose the folder containing level JSON files",
            _selectedFolderPath,
            "");
#elif UNITY_STANDALONE_WIN
        folderPath = WindowsFolderPicker.Open("Choose the folder containing level JSON files");
#else
        Debug.LogError("The level folder picker is supported only in the Unity Editor and Windows builds.", this);
        return false;
#endif
        return !string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath);
    }

#if UNITY_STANDALONE_WIN
    private static class WindowsFolderPicker
    {
        private const uint BrowseForFileSystemDirectories = 0x00000001;
        private const uint UseNewDialogStyle = 0x00000040;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct BrowseInfo
        {
            public System.IntPtr owner;
            public System.IntPtr root;
            public System.IntPtr displayName;
            public System.IntPtr title;
            public uint flags;
            public System.IntPtr callback;
            public System.IntPtr callbackParameter;
            public int image;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHBrowseForFolderW")]
        private static extern System.IntPtr SHBrowseForFolder(ref BrowseInfo browseInfo);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHGetPathFromIDListW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SHGetPathFromIDList(System.IntPtr itemIdList, System.IntPtr path);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(System.IntPtr memory);

        [DllImport("user32.dll")]
        private static extern System.IntPtr GetActiveWindow();

        public static string Open(string title)
        {
            System.IntPtr displayName = Marshal.AllocHGlobal(260 * sizeof(char));
            System.IntPtr nativeTitle = Marshal.StringToHGlobalUni(title);
            try
            {
                var browseInfo = new BrowseInfo
                {
                    owner = GetActiveWindow(),
                    displayName = displayName,
                    title = nativeTitle,
                    flags = BrowseForFileSystemDirectories | UseNewDialogStyle
                };

                System.IntPtr itemIdList = SHBrowseForFolder(ref browseInfo);
                if (itemIdList == System.IntPtr.Zero)
                    return "";

                try
                {
                    System.IntPtr path = Marshal.AllocHGlobal(260 * sizeof(char));
                    try
                    {
                        for (int index = 0; index < 260; index++)
                            Marshal.WriteInt16(path, index * sizeof(char), 0);

                        return SHGetPathFromIDList(itemIdList, path)
                            ? Marshal.PtrToStringUni(path)
                            : "";
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(path);
                    }
                }
                finally
                {
                    CoTaskMemFree(itemIdList);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(displayName);
                Marshal.FreeHGlobal(nativeTitle);
            }
        }
    }
#endif
}