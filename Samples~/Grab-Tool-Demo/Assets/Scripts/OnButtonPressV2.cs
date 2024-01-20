using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

/// <summary>
/// Checks for button input on an input action
/// </summary>
public class OnButtonPressV2 : MonoBehaviour
{
    [SerializeField] [Tooltip("The Input System Action that will be used to read button press data.")]
    private InputActionProperty buttonAction = new InputActionProperty(new InputAction("Button Press"));

    // When the button is pressed
    public UnityEvent onPress = new();

    // When the button is released
    public UnityEvent onRelease = new();


    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnEnable()
    {
        buttonAction.EnableDirectAction();
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnDisable()
    {
        buttonAction.DisableDirectAction();
    }

    private void Update()
    {
        var action = buttonAction.action;

        if (action.WasPressedThisFrame() && action.IsPressed())
        {
            onPress.Invoke();
        }

        if (action.WasReleasedThisFrame() && !action.IsPressed())
        {
            onRelease.Invoke();
        }
    }
}