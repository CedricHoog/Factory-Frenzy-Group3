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
            foreach (JObject o in JArray.Parse(json))
            {
                var e = new LevelEntry
                {
                    Id = (int)o["Id"],
                    Name = (string)o["Name"],
                    Position = ReadVec(o["Position"] ?? o["StartingPosition"])
                };

                if (o["Size"] != null) { e.HasSize = true; e.Size = ReadVec(o["Size"]); }
                if (o["EndingPosition"] != null) e.EndPosition = ReadVec(o["EndingPosition"]);
                if (o["MovementTime"] != null) e.MovementTime = (float)o["MovementTime"];

                JToken ori = o["Orientation"];
                if (ori != null)
                    e.Rotation = ori.Type == JTokenType.Object
                        ? Quaternion.Euler(ReadVec(ori))
                        : Quaternion.Euler(0f, (float)ori, 0f);   // nombre = rotation en Y

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