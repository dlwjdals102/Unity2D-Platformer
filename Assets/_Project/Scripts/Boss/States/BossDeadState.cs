using UnityEngine;

public class BossDeadState : BossState
{
    public BossDeadState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter(); // "dead" 애니메이션 Bool을 켭니다.

        // 1. 물리적 상태 고정 (시체화)
        boss.Movement.SetVelocity(0f, 0f);
        boss.Movement.RB.bodyType = RigidbodyType2D.Static; // 완전히 고정

        Collider2D coll = boss.GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        // 2. 처치 기록에 따른 조건부 연출
        if (!DataManager.Instance.sessionData.isBossDefeated)
        {
            // [최초 처치 시에만 실행되는 로직]
            DataManager.Instance.sessionData.isBossDefeated = true; // 기록 갱신

            Debug.Log("[Boss] 최초 처치! 화려한 사망 연출을 실행합니다.");

            // 여기에 카메라 쉐이크나 슬로우 모션 코드를 넣으세요.
            // 예: FeedbackManager.Instance.TriggerCameraShake(5f, 0.5f);
        }
        else
        {
            // [씬 재진입 시 실행되는 로직]
            Debug.Log("[Boss] 이미 처치된 보스입니다. 연출 없이 시체 상태를 유지합니다.");
        }
    }

}