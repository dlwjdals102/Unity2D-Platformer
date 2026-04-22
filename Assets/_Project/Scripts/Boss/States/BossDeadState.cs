using UnityEngine;

public class BossDeadState : BossState
{
    public BossDeadState(Boss entity, StateMachine<Boss> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter(); // "dead" 애니메이션 Bool을 켭니다.

        // 1. 움직임 완전 고정 및 충돌체 제거 (플레이어가 시체에 막히지 않도록)
        boss.Movement.SetVelocity(0f, boss.Movement.RB.linearVelocity.y);
        boss.Movement.RB.bodyType = RigidbodyType2D.Kinematic;

        Collider2D coll = boss.GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        Debug.Log("[Boss] 보스 처치! 사망 연출 시작.");

        // TODO: 타격감 극대화를 위한 화면 슬로우(Time.timeScale = 0.5f), 
        // 카메라 흔들림(Camera Shake), 연속 폭발 파티클 등을 여기에 추가하세요!
    }

    public override void Update()
    {
        base.Update();

        // 사망 애니메이션이 완전히 끝나면 오브젝트를 끕니다. (혹은 시체로 남겨둡니다)
        if (isAnimationFinished)
        {
            boss.gameObject.SetActive(false); // Destroy 대신 SetActive(false) 추천
        }
    }
}