using UnityEngine;

public class ElementView : MonoBehaviour
{
    public GameElement Element { get; private set; }
    public int ElementId;
    public bool IsLocked = true;

    public void Bind(GameElement e)
    {
        Element = e;
        ElementId = e.id;
        transform.localScale = e.size;
    }

    // data → scene object
    public void Apply()
    {
        transform.localPosition = Element.position;
        transform.localRotation = Quaternion.Euler(Element.rotation);
        transform.localScale = Element.size;
    }

    // scene object → data
    public void WriteBack()
    {
        Element.position = transform.localPosition;
        Element.rotation = transform.localEulerAngles;
    }
}