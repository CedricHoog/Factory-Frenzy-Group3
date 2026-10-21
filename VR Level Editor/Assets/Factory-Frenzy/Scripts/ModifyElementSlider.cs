using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ModifyElementSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private string format = "0.0";
    [SerializeField] private string suffix = "x";

    // Fired only when the USER moves the slider, never by SetValueWithoutNotify
    public event Action<float> ValueChanged;

    private Slider slider;

    // Lazy, so it also works if called before Awake (inactive hierarchy)
    private Slider S => slider != null ? slider : (slider = GetComponent<Slider>());

    public float Value => S.value;

    void OnEnable()
    {
        S.onValueChanged.AddListener(OnSliderMoved);
        UpdateLabel(S.value);
    }

    void OnDisable() => S.onValueChanged.RemoveListener(OnSliderMoved);

    // Load a value from the selected element: moves the slider and the label, fires nothing
    public void SetValueWithoutNotify(float value)
    {
        S.SetValueWithoutNotify(value);
        UpdateLabel(S.value);          // S.value is clamped to the slider's min/max
    }

    private void OnSliderMoved(float value)
    {
        UpdateLabel(value);
        ValueChanged?.Invoke(value);
    }

    private void UpdateLabel(float value)
    {
        if (label != null)
            label.text = value.ToString(format, CultureInfo.InvariantCulture) + suffix;
    }
}