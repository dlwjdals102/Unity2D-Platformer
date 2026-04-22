using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationEventHandler : MonoBehaviour
{
    // 외부(PlayerController, Enemy 등)에서 구독할 C# 이벤트
    public event Action OnAttackTriggered;
    public event Action OnAnimationFinished;

    // ==========================================
    // 유니티 애니메이션 창(Animation Event)에서 호출할 함수들
    // ==========================================

    public void TriggerAttack()
    {
        // 구독자가 있으면 실행 (방아쇠 당김)
        OnAttackTriggered?.Invoke();
    }

    public void TriggerAnimationFinish()
    {
        // 애니메이션이 끝났음을 알림
        OnAnimationFinished?.Invoke();
    }
}