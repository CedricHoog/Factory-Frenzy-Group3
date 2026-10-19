using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



[RequireComponent(typeof(Button))]
public class ElementButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string elementName;
    [SerializeField] private TooltipPanel panel;
    [SerializeField] private GameElementManager manager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => manager.CreateGameElement(elementName));
    }

    public void OnPointerEnter(PointerEventData eventData) => panel.ShowHover(elementName);
    public void OnPointerExit(PointerEventData eventData) => panel.EndHover();
}