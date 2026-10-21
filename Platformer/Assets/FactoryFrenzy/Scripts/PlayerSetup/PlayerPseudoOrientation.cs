
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerPseudoOrientation : NetworkBehaviour
{
    [SerializeField] private Transform billboardRoot;
    [SerializeField] private TMP_Text usernameText;

    private Camera cam;

    private void LateUpdate()
    {
        if (!billboardRoot.gameObject.activeInHierarchy) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        billboardRoot.rotation = cam.transform.rotation;
    }
}
