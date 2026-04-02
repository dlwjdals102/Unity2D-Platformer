using UnityEngine;

public class StateMachine<T> where T : Entity
{
    public State<T> CurrentState { get; private set; }

    public void Initialize(State<T> startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(State<T> newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}