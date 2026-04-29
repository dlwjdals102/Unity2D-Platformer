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

    /// <summary>공격 타격 프레임에서 호출 (상태 머신에 알림).</summary>
    public void TriggerAttack()
    {
        OnAttackTriggered?.Invoke();
    }

    /// <summary>애니메이션 종료 프레임에서 호출 (상태 머신에 알림).</summary>
    public void TriggerAnimationFinish()
    {
        // 애니메이션이 끝났음을 알림
        OnAnimationFinished?.Invoke();
    }

    /// <summary>
    /// 걷기/달리기 애니메이션의 발 닿는 프레임에서 호출.
    /// AudioManager에 등록된 발소리를 즉시 재생합니다.
    /// </summary>
    public void OnFootstep()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance?.Play("SFX_FootStep");
    }
}