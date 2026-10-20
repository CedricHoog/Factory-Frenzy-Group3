using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



[RequireComponent(typeof(Button))]
public class InteractionButton : MonoBehaviour
{
    [SerializeField] private string buttonName; //lock, delete, swap
    private GameElementManager manager;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => manager.InteractionUtil(buttonName)); //check if ok
        manager = FindFirstObjectByType<GameElementManager>();
    }

}