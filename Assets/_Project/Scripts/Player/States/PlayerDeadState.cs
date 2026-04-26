using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeadState : PlayerState
{
    private int originalLayer; // 원래 레이어를 저장할 변수

    public PlayerDeadState(PlayerController entity, StateMachine<PlayerController> stateMachine, string animBoolName)
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 1. 죽었으므로 모든 물리적인 움직임을 즉시 멈춥니다.
        player.Movement.ZeroVelocity();

        // 현재 레이어를 백업해두고, 무적 레이어로 변경합니다.
        originalLayer = player.gameObject.layer;
        player.gameObject.layer = LayerMask.NameToLayer(Define.LayerNames.IgnoreRaycast);
    }

    public override void Update()
    {
        base.Update();

        // 3. 사망 애니메이션이 완전히 끝나면?
       /* if (isAnimationFinished)
        {
            // 현재 활성화된 씬의 이름을 가져와서 다시 로드(재도전)합니다.
            GameManager.Instance.RespawnPlayer();
        }*/
    }

    public override void Exit()
    {
        base.Exit();

        // 상태를 빠져나갈 때(예: 부활) 원래 레이어로 원상 복구시킵니다.
        player.gameObject.layer = originalLayer;
    }
}