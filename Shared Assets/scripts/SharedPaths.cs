using System.IO;
using UnityEngine;


public static class SharedPaths
{
    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    public static string Root
    {
#if UNITY_EDITOR
        get => Path.GetFullPath(Path.Combine(ProjectRoot, "..", "Shared Assets"));
#else
        get => Application.persistentDataPath;
#endif
    }

    public static string Levels => Path.Combine(Root, "levels");
}