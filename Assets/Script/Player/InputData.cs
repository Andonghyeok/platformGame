using UnityEngine;

public enum InputType { None, Move, Jump, Dash, Attack }
public enum InputPhase { Started, Canceled }
public enum PlayerState { Idle, Moving, JumpCharging, DashCharging, Dashing, Attacking, Stunned }

[System.Serializable]
public struct InputData
{
    public InputType type;
    public InputPhase phase;
    public float timestamp;
    public Vector2 direction;

    public InputData(InputType type, InputPhase phase, Vector2 direction)
    {
        this.type = type;
        this.phase = phase;
        this.timestamp = Time.time;
        this.direction = direction;
    }
}