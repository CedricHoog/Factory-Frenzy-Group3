using System;
using UnityEngine;

public class ElementView : MonoBehaviour
{
    public GameElement Element { get; private set; }
    public int ElementId;
    public bool IsUnlocked = true;
    
    public event Action<int> Grabbed; 
    public void NotifyGrabbed() => Grabbed?.Invoke(Element.id);
    
    public event Action<ElementView> UnGrabbed; 
    public void NotifyUnGrabbed() => UnGrabbed?.Invoke(this);
    
    public void Bind(GameElement e)
    {
        Element = e;
        ElementId = e.id;
        transform.localScale = e.size;
    }

    // data → scene object
    public void Apply()
    {
        //NOTE might cause an issue when loading a level.
        //If so, re-add this line & don't apply this transformation when using a slider on the modify menu
        //transform.localPosition = Element.position; 
        transform.localRotation = Quaternion.Euler(Element.rotation);
        transform.localScale = Element.size;
    }

    // scene object → data
    public void WriteBack()
    {
        Element.position = transform.localPosition;
        Element.rotation = transform.localEulerAngles;
    }
    
    // return the combined bounds of an element's colliders
    public Bounds GetWorldBounds()
    {
        var cols = GetComponentsInChildren<Collider>();
        var b = new Bounds(transform.position, Vector3.zero);
        foreach (var c in cols) b.Encapsulate(c.bounds);
        return b;
    }
}