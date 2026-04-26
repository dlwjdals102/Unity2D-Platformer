using UnityEngine;

public class StateMachine<T> where T : Entity
{
    public State<T> CurrentState { get; private set; }
    public State<T> PreviousState { get; private set; }

    /// <summary>
    /// 상태 머신을 초기 상태로 가동합니다. 반드시 한 번만 호출되어야 합니다.
    /// </summary>
    public void Initialize(State<T> startState)
    {
        if (startState == null)
        {
            Debug.LogError("[StateMachine] Initialize에 null 상태가 전달되었습니다.");
            return;
        }

        CurrentState = startState;
        CurrentState.Enter();
    }

    /// <summary>
    /// 상태를 전환합니다. null 전달, Initialize 미호출, 동일 상태 재진입을 모두 방어합니다.
    /// </summary>
    public void ChangeState(State<T> newState)
    {
        // 1. null 가드: 잘못된 상태로 전환 시도 시 무시
        if (newState == null)
        {
            Debug.LogError("[StateMachine] ChangeState에 null 상태가 전달되었습니다.");
            return;
        }

        // 2. Initialize 미호출 가드: CurrentState가 null인 상태에서 ChangeState가 먼저 호출되는 경우
        if (CurrentState == null)
        {
            Debug.LogWarning("[StateMachine] Initialize가 호출되지 않은 상태에서 ChangeState가 호출되었습니다. Initialize로 처리합니다.");
            Initialize(newState);
            return;
        }

        // 3. 동일 상태 재진입 가드: 같은 상태로 전환 시 Exit/Enter 사이클을 막아 무한 루프 방지
        //    (의도적으로 같은 상태를 다시 진입하고 싶다면 ReEnter() 사용)
        if (CurrentState == newState) return;

        PreviousState = CurrentState;
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    /// <summary>
    /// 현재 상태를 강제로 다시 진입합니다 (Exit → Enter).
    /// 콤보 공격 다음 단계로 같은 AttackState를 다시 호출할 때 등에 사용합니다.
    /// </summary>
    public void ReEnter()
    {
        if (CurrentState == null) return;
        CurrentState.Exit();
        CurrentState.Enter();
    }
}