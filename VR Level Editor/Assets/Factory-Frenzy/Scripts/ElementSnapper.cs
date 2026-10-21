using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine;

public static class ElementSnapper
{
    // Returns where the element should be. Does not move anything itself.
    public static Pose Solve(ElementView moving, IEnumerable<ElementView> others, SnapSettings s)
    {
        return new Pose(); //TODO
    }
}