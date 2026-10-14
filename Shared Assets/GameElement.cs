    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    [Serializable] public class ElementText { public string type, name, description; }
    [Serializable] public class ElementTexts { public string language; public List<ElementText> elements; }

    public static class GameElementTexts
    {
        static Dictionary<string, ElementText> map;
        //TODO DONT FORGET TO LOAD THIS ON STARTUP

        public static void Load(string lang = "en_us")
        {
            var asset = Resources.Load<TextAsset>($"GameElementTexts.{lang}");
            var data = JsonUtility.FromJson<ElementTexts>(asset.text);
            map = new();
            foreach (var e in data.elements) map[e.type] = e;
        }

        public static ElementText Get(Type t) => map[t.Name];
    }

    [Serializable]
    public abstract class GameElement
    {
        //Possible parameters : position, size, orientation, endingPosition, movementTime
        //force could also be added for PlatformTrampoline, Bumper. Thought it doesn't seems required.

        public int id; //should be unique accross all game elements loaded. 

        public Vector3 position;
        public Vector3 size = Vector3.one;
        public Vector3 rotation;
        
        public string Name => GameElementTexts.Get(GetType()).name;
        public string Description => GameElementTexts.Get(GetType()).description;

    }

    /* All 9 Game Elements */
    [Serializable]
    public class Platform : GameElement
    {
    }

    [Serializable]
    public class PlatformMoving : GameElement
    {
        public float movementTime = 2f;
        public Vector3 endingPosition;
    }

    [Serializable]
    public class PlatformTrampoline : GameElement
    {
        //we are not required to change the force so could go unused
        public float force = 10f;          // default value, so it can go unused
    }

    [Serializable]
    public class TrapBumper : GameElement
    {
        public float force = 10f;          // default value, so it can go unused
    }

    [Serializable]
    public class TrapFan : GameElement
    {
        public float force = 10f;          // default value, so it can go unused
        public Vector3 windZone = Vector3.one;          // default value, so it can go unused
    }

    [Serializable]
    public class TrapThrower : GameElement
    {
        public float range = 10f;          // default value, so it can go unused
    }

    [Serializable]
    public class Checkpoint : GameElement
    {
    }

    [Serializable]
    public class PlatformStart : GameElement
    {
    }

    [Serializable]
    public class PlatformEnd : GameElement
    {
    }