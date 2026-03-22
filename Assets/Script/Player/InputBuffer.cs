using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBuffer : MonoBehaviour
{
    Queue<InputData> buffer = new Queue<InputData>();

    public float bufferTime = 0.2f;

    public Vector2 MoveInput { get; private set; }

    void Update()
    {
        while (buffer.Count > 0 && Time.time - buffer.Peek().timestamp > bufferTime)
            buffer.Dequeue();
    }

    public void AddInput(InputType type, InputPhase phase)
    {
        buffer.Enqueue(new InputData(type, phase, MoveInput));
    }

    public InputData? Peek()
    {
        if (buffer.Count == 0) return null;
        return buffer.Peek();
    }

    public InputData? Pop()
    {
        if (buffer.Count == 0) return null;
        return buffer.Dequeue();
    }

    // Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) AddInput(InputType.Jump, InputPhase.Started);
        if (context.canceled) AddInput(InputType.Jump, InputPhase.Canceled);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started) AddInput(InputType.Dash, InputPhase.Started);
        if (context.canceled) AddInput(InputType.Dash, InputPhase.Canceled);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started) AddInput(InputType.Attack, InputPhase.Started);
    }
}