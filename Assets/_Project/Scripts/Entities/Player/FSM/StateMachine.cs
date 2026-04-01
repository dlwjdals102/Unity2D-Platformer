using UnityEngine;

public class StateMachine
{
    public State CurrentState { get; private set; }

    // 초기 상태 설정
    public void Initialize(State startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    // 상태 전환
    public void ChangeState(State newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
