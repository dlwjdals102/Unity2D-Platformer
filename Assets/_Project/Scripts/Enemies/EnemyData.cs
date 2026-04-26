using UnityEngine;

/// <summary>
/// 모든 적의 공통 데이터 (이동, 감지, 패트롤 타이머 등).
/// 각 적 타입은 이 클래스를 상속받아 자신만의 공격 데이터 + AttackState 생성 메서드를 정의합니다.
/// 추상 클래스이므로 직접 SO 인스턴스를 만들 수 없고,
/// MeleeEnemyData, RangedEnemyData 같은 구체 클래스로만 SO를 생성합니다.
/// </summary>
public abstract class EnemyData : EntityData
{
    [Header("Enemy AI")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionDistance = 8f;
    public float attackDistance = 1.5f;

    // 시야에서 사라진 후 추적을 유지하는 시간
    public float aggroDuration = 2.0f;

    [Header("Enemy Combat")]
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public float specialAttackCooldown = 8f;

    [Header("Patrol Timers")]
    public float patrolDuration = 3f;     // 평소에 걷는 시간
    public float idleDuration = 2f;       // 평소에 가만히 쉬는 시간
    public float turnWaitDuration = 0.5f; // 절벽/벽을 만났을 때 멈칫! 하고 바라보는 시간

    /// <summary>
    /// 이 데이터에 맞는 공격 상태(EnemyAttackState)를 생성하여 반환합니다.
    /// 각 적 타입(Melee/Ranged/Magic 등)이 자신의 공격 행동을 직접 결정합니다.
    /// 이 패턴 덕분에 새 적을 추가할 때 Enemy.cs를 수정할 필요가 없습니다.
    /// </summary>
    public abstract EnemyAttackStateBase CreateAttackState(Enemy owner, StateMachine<Enemy> stateMachine);
}