using UnityEngine;
using System.Collections.Generic;

public class QueueInputSystem : MonoBehaviour
{
    private Queue<InputData> _inputBuffer = new Queue<InputData>();
    [SerializeField] private float bufferTime = 0.2f;

    private void Update()
    {
        while (_inputBuffer.Count > 0 && Time.time - _inputBuffer.Peek().timestamp > bufferTime)
        {
            _inputBuffer.Dequeue();
        }
    }

    public void AddInput(InputType type, InputPhase phase, Vector2 dir)
    {
        InputData newData = new InputData(type, phase, dir);
        _inputBuffer.Enqueue(newData);
        Debug.Log($"ÀÔ·Â ±â·ÏµÊ: {type} ({phase})");
    }

    public InputData? PeekInput()
    {
        if (_inputBuffer.Count > 0) return _inputBuffer.Peek();
        return null;
    }

    public void ConsumeInput()
    {
        if (_inputBuffer.Count > 0) _inputBuffer.Dequeue();
    }
}