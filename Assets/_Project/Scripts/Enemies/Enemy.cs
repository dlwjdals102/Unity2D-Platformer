using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Enemy : Entity
{
    // ==========================================
    // 1. 상태 머신 및 상태 인스턴스
    // ==========================================
    public StateMachine<Enemy> stateMachine { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyHurtState HurtState { get; private set; }
    public EnemyDeadState DeadState { get; private set; }

    [Header("AI Settings")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 5f;
    public float detectionDistance = 8f;

    [Header("Detection Setup")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform ledgeCheck; // 낭떠러지 감지 전용 위치 (발끝 앞쪽)

    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackDistance = 1.5f; // 공격 사거리
    public float attackCooldown = 2.0f; // 공격을 한 번 하면 2초 동안 쉰다
    public float lastAttackTime;        // 마지막으로 공격한 시간 기록

    protected override void Awake()
    {
        base.Awake(); // 부모(Entity)의 Awake 호출: RB, Anim, 체력 자동 초기화

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine<Enemy>();
        PatrolState = new EnemyPatrolState(this, stateMachine, "Move");
        ChaseState = new EnemyChaseState(this, stateMachine, "Move");
        AttackState = new EnemyAttackState(this, stateMachine, "Attack");
        HurtState = new EnemyHurtState(this, stateMachine, "Hurt");
        DeadState = new EnemyDeadState(this, stateMachine, "Dead");
    }

    private void Start()
    {
        stateMachine.Initialize(PatrolState);
    }

    private void Update()
    {
        stateMachine.CurrentState?.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState?.FixedUpdate();
    }

    // ==========================================
    // AI 센서 (눈과 귀)
    // ==========================================
    public bool IsLedgeDetected() => Physics2D.Raycast(ledgeCheck.position, Vector2.down, 1f, groundLayer);


    public bool IsPlayerInSight()
    {
        // 벽(groundLayer)과 플레이어(playerLayer)를 모두 감지하도록 LayerMask를 합칩니다.
        LayerMask mask = playerLayer | groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, transform.right, detectionDistance, mask);

        if (hit.collider != null)
        {
            // 광선에 맞은 첫 번째 물체의 레이어가 플레이어 레이어인지 확인합니다!
            // (만약 벽 뒤에 숨어있다면 광선이 벽에 먼저 맞으므로 false가 반환됩니다)
            if ((playerLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
            {
                return true;
            }
        }

        return false;
    }
    // 플레이어가 '공격 사거리' 내에 있는가?
    public bool IsPlayerInAttackRange()
    {
        LayerMask mask = playerLayer | groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, transform.right, attackDistance, mask);

        if (hit.collider != null && (playerLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
        {
            return true;
        }
        return false;
    }

    // ==========================================
    // Entity의 추상 메서드 구현 (피격)
    // ==========================================
    public override void TakeDamage(float damage)
    {
        // 이미 죽은 상태라면 데미지를 무시합니다 (시체 다단히트 방지)
        if (stateMachine.CurrentState == DeadState) return;
        if (stateMachine.CurrentState == HurtState) return;

        currentHealth -= damage;
        Debug.Log($"[적 피격] 남은 체력: {currentHealth}");

        // 체력이 깎였으니 UI들에게 방송 송출!
        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            // 체력이 다 닳았다면 사망 상태로 강제 전환!
            stateMachine.ChangeState(DeadState);
        }
        else
        {
            // 체력이 남았다면 피격(넉백) 상태로 강제 전환!
            stateMachine.ChangeState(HurtState);
        }
    }

    // ==========================================
    // 애니메이션 이벤트 전용 브릿지 함수들
    // ==========================================
    // 공격 애니메이션의 '타격 프레임'에 호출할 함수
    public void TriggerAttack()
    {
        // 공격 사거리 내에 있는 플레이어 감지 (OverlapCircle을 써도 좋고, Raycast를 써도 됩니다)
        Collider2D playerCollider = Physics2D.OverlapCircle(wallCheck.position, attackDistance, playerLayer);

        if (playerCollider != null)
        {
            IDamageable damageable = playerCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
        }
    }

    // 공격 애니메이션의 '마지막 프레임'에 호출할 함수
    public void AnimationFinishTrigger()
    {
        stateMachine.CurrentState?.AnimationFinishTrigger();
    }

    // 에디터에서 센서 시각화

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * detectionDistance); // 시야

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(wallCheck.position, attackDistance); // 공격 범위 원형 시각화
        }

        if (ledgeCheck != null)
        {
            Gizmos.color = Color.blue; // 낭떠러지 감지선은 파란색
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * 1f);
        }
    }
}
