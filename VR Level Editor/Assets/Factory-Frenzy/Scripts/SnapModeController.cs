using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine;

public class SnapModeController : MonoBehaviour
{
    [SerializeField] private InputActionProperty toggleAction;

    public bool IsOn { get; private set; }
    public event Action<bool> Changed;

    void OnEnable()
    {
        toggleAction.action.Enable();
        toggleAction.action.performed += OnPressed;
    }

    void OnDisable()
    {
        toggleAction.action.performed -= OnPressed;
    }

    private void OnPressed(InputAction.CallbackContext ctx) => Toggle();

    // Public so a UI button's OnClick could call it too
    public void Toggle() => SetOn(!IsOn);

    public void SetOn(bool on)
    {
        if (IsOn == on) return;
        IsOn = on;
        Debug.Log($"Snap mode: {(IsOn ? "ON" : "OFF")}");   // TODO remove
        Changed?.Invoke(IsOn);
    }
}

/*
public class SnapModeController : MonoBehaviour
{
    [SerializeField] private InputActionProperty toggleAction;
    public bool IsOn { get; private set; }
    public event Action<bool> Changed;

    // OnEnable/OnDisable: subscribe toggleAction.performed → Toggle()
    public void Toggle() { IsOn = !IsOn; Changed?.Invoke(IsOn); }   // NOTE could be called from an UI button
}*/