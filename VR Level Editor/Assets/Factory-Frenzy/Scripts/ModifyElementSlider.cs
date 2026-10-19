using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ModifyElementSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private string format = "0.0";
    [SerializeField] private string suffix = "x";

    private Slider slider;

    void Awake() => slider = GetComponent<Slider>();

    void OnEnable()
    {
        slider.onValueChanged.AddListener(UpdateLabel);
        UpdateLabel(slider.value);
    }

    void OnDisable() => slider.onValueChanged.RemoveListener(UpdateLabel);

    private void UpdateLabel(float value)
    {
        label.text = value.ToString(format) + suffix;
    }
}