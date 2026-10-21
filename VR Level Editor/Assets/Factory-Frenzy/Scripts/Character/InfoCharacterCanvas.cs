
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InfoCharacterCanvas : MonoBehaviour
{
    [SerializeField] private Transform canvasRoot;
    [SerializeField] private XRGrabInteractable parentGrabInteractable;
    [SerializeField] private Camera cam;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        if (parentGrabInteractable == null)
            parentGrabInteractable = GetComponentInParent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (parentGrabInteractable != null)
            parentGrabInteractable.selectEntered.AddListener(OnParentGrabbed);
    }

    private void OnDisable()
    {
        if (parentGrabInteractable != null)
            parentGrabInteractable.selectEntered.RemoveListener(OnParentGrabbed);
    }

    private void LateUpdate()
    {
        if (cam != null && canvasRoot != null)
            canvasRoot.rotation = cam.transform.rotation;
    }

    private void OnParentGrabbed(SelectEnterEventArgs args)
    {
        if (canvasRoot != null)
            canvasRoot.gameObject.SetActive(false);
    }
}
