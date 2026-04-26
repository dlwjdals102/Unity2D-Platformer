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

        // 2. 처치 기록에 따른 조건부 연출 (DataManager null 가드)
        if (DataManager.Instance == null || DataManager.Instance.sessionData == null) return;

        if (!DataManager.Instance.sessionData.isBossDefeated)
        {
            // [최초 처치 시에만 실행되는 로직]
            DataManager.Instance.sessionData.isBossDefeated = true;

            // 보스 처치는 큰 마일스톤이므로 디스크에 즉시 저장합니다.
            // 만약 이 직후 게임이 비정상 종료되어도 처치 기록이 보존됩니다.
            DataManager.Instance.SaveToFile();

            // TODO: 카메라 쉐이크, 슬로우 모션, 보상 드롭 등 화려한 연출 추가
            // 예: FeedbackManager.Instance?.TriggerCameraShake(5f);
        }
        // else: 이미 처치된 보스 - 연출 없이 시체 상태만 유지

    }

}