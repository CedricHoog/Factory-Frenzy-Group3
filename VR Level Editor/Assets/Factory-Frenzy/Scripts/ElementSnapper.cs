using System.Collections.Generic;
using UnityEngine;

public static class ElementSnapper
{
    // Rounds each Euler angle to the nearest multiple of step. step <= 0 leaves it unchanged.
    // Call this BEFORE Solve, because the world bounds depend on the rotation.
    public static Quaternion SnapRotation(Quaternion rotation, float step)
    {
        //TODO TEST
        Debug.Log($"SnapRotation Quaternion {rotation}, float {step}");
        if (step <= 0f) return rotation;

        Vector3 e = rotation.eulerAngles;
        e.x = Mathf.Round(e.x / step) * step;
        e.y = Mathf.Round(e.y / step) * step;
        e.z = Mathf.Round(e.z / step) * step;
        Debug.Log($"{e.x} {e.y},  {e.z}");
        return Quaternion.Euler(e);
    }

    // Returns where the element should be. Does not move anything itself.
    // If no other element is within snapDistance, the returned pose is the current one.
    public static Pose Solve(ElementView moving, IEnumerable<ElementView> others, SnapSettings settings)
    {
        Transform t = moving.transform;
        
        Vector3 offset = Vector3.zero;

        Physics.SyncTransforms();                       // make sure collider bounds are up to date
        Bounds b = moving.GetWorldBounds();

        float d = settings.snapDistance;
        
        Bounds reach = b;
        reach.Expand(d * 2f);
        
        Vector3 best = new Vector3(d, d, d);

        foreach (ElementView other in others)
        {
            if (other == null) continue;

            Bounds o = other.GetWorldBounds();
            if (!reach.Intersects(o)) continue;

            for (int axis = 0; axis < 3; axis++)        // 0 = X, 1 = Y, 2 = Z
            {
                float bMin = b.min[axis], bMax = b.max[axis];
                float oMin = o.min[axis], oMax = o.max[axis];

                Consider(oMax - bMin, axis, ref offset, ref best);   // my low face touches its high face
                Consider(oMin - bMax, axis, ref offset, ref best);   // my high face touches its low face
                Consider(oMin - bMin, axis, ref offset, ref best);   // low edges flush
                Consider(oMax - bMax, axis, ref offset, ref best);   // high edges flush
            }
        }

        Debug.Log($"{t.position} + {offset}, {t.rotation}");
        return new Pose(t.position + offset, t.rotation);
    }

    // Keeps the smallest move on this axis
    private static void Consider(float delta, int axis, ref Vector3 offset, ref Vector3 best)
    {
        float dist = Mathf.Abs(delta);
        if (dist < best[axis])
        {
            best[axis] = dist;
            offset[axis] = delta;
        }
    }
}