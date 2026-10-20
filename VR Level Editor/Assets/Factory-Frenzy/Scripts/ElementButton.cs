using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



[RequireComponent(typeof(Button))]
public class ElementButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string elementName;
    [SerializeField] private TooltipPanel panel;
    private GameElementManager manager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => manager.CreateGameElement(elementName));
        manager = FindFirstObjectByType<GameElementManager>();
    }

    public void OnPointerEnter(PointerEventData eventData) => panel.ShowHover(elementName);
    public void OnPointerExit(PointerEventData eventData) => panel.EndHover();
}