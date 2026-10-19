using UnityEngine;

public class PseudoRotation : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;
    }
}