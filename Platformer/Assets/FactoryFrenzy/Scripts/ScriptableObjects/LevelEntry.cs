using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class LevelEntry
{
    public int Id;
    public string Name;
    public Vector3 Position;
    public Quaternion Rotation = Quaternion.identity;
    public bool HasSize;
    public Vector3 Size = Vector3.one;
    public Vector3 EndPosition;       // PlatformMoving
    public float MovementTime;        // PlatformMoving
}

public static class LevelParser
{
    public static bool TryParse(string json, out List<LevelEntry> entries, out string error)
    {
        entries = new List<LevelEntry>();
        error = "";
        try
        {
            JObject root = JObject.Parse(json);

            JArray elements = root["elements"] as JArray;
            if (elements == null)
            {
                error = "Champ \"elements\" manquant.";
                return false;
            }

            foreach (JObject o in elements)
            {
                var e = new LevelEntry
                {
                    Id = (int)o["id"],
                    Name = (string)o["name"],
                    Position = ReadVec(o["position"]),
                    Rotation = Quaternion.Euler(ReadVec(o["rotation"]))
                };

                if (o["size"] != null)
                {
                    e.HasSize = true;
                    e.Size = ReadVec(o["size"]);
                }

                entries.Add(e);
            }
            return true;
        }
        catch (System.Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static Vector3 ReadVec(JToken t) =>
        t == null ? Vector3.zero : new Vector3((float)t["x"], (float)t["y"], (float)t["z"]);
}

public static class LevelSelection
{
    public static string FileName { get; private set; } = "";
    public static string Json { get; private set; } = "";
    public static bool HasLevel => !string.IsNullOrEmpty(Json);

    public static void Set(string fileName, string json) { FileName = fileName; Json = json; }
    public static void Clear() { FileName = ""; Json = ""; }
}