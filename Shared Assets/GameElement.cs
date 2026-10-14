using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class GameElement
{
    //Possible parameters : position, size, orientation, endingPosition, movementTime
    //force could also be added for PlatformTrampoline, Bumper. Thought it doesn't seems required.

    public int id; //should be unique accross all game elements loaded. 

    public Vector3 position;
    public Vector3 size = Vector3.one;
    public Vector3 rotation;
    
    public abstract string Name { get; }
    public abstract string Description { get; }

}

/* All 9 Game Elements */
[Serializable]
public class Platform : GameElement
{
    public override string Name => "Platform";
    public override string Description => "Sample Descr.";
}

[Serializable]
public class PlatformMoving : GameElement
{
    public override string Name => "Moving platform";
    public override string Description => "Sample Descr.";
    public float movementTime = 2f;
    public Vector3 endingPosition;
}

[Serializable]
public class PlatformTrampoline : GameElement
{
    //we are not required to change the force so could go unused
    public override string Name => "Trampoline Platform";
    public override string Description => "Sample Descr.";
    public float force = 10f;          // default value, so it can go unused
}

[Serializable]
public class TrapBumper : GameElement
{
    public override string Name => "Bumper";
    public override string Description => "Sample Descr.";
    public float force = 10f;          // default value, so it can go unused
}

[Serializable]
public class TrapFan : GameElement
{
    public override string Name => "Fan";
    public override string Description => "Sample Descr.";
    //we are not required to change the force & activation zone so could go unused
    public float force = 10f;          // default value, so it can go unused
    public Vector3 windZone = Vector3.one;          // default value, so it can go unused
}

[Serializable]
public class TrapThrower : GameElement
{
    public override string Name => "Thrower";
    public override string Description => "Sample Descr.";
    //we are not required to change the range so could go unused
    public float range = 10f;          // default value, so it can go unused
}

[Serializable]
public class Checkpoint : GameElement
{
    public override string Name => "Checkpoint";
    public override string Description => "Sample Descr.";
}

[Serializable]
public class PlatformStart : GameElement
{
    public override string Name => "Start Platform";
    public override string Description => "Sample Descr.";
}

[Serializable]
public class PlatformEnd : GameElement
{
    public override string Name => "End Platform";
    public override string Description => "Sample Descr.";
}