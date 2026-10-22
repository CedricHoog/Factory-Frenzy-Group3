using UnityEngine;
using UnityEngine.Serialization;

public class SnapSettings : MonoBehaviour
{
    [SerializeField] public float snapDistance = 0.3f;   // distance at which snapping kicks in
    [SerializeField] public float rotationStep = 10f;    //unused currently
}