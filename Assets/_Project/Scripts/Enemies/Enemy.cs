using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class Enemy : Entity
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

    [Header("Data")]
    [field: SerializeField] public EnemyData Data { get; protected set; } // 데이터 컨테이너 연결

    [Header("Base AI Settings")]
    [HideInInspector] public float lastAttackTime;

    // 감지 센서 세팅 추가!
    [Header("Detection Setup")]
    public LayerMask playerLayer;        // 플레이어를 감지할 레이어
    public Transform ledgeCheck;         // 절벽 감지용 빈 오브젝트 (몬스터 앞쪽 발밑)
    public Transform playerCheck;        // 시야 감지 기준점 (보통 몬스터의 눈 위치)
    public float ledgeCheckDistance = 0.5f;

    protected override void Awake()
    {
        base.Awake(); // 부모(Entity)의 Awake 호출: RB, Anim, 체력 자동 초기화

        // SO 데이터 주입 및 초기화 (무적 시간 없음)
        if (Data != null)
        {
            Health.Initialize(Data.maxHealth);
        }

        // 피격 및 사망 이벤트 구독
        if (Health != null)
        {
            Health.OnTakeDamage += HandleTakeDamage;
            Health.OnDeath += HandleDeath;
        }

        // 애니메이션 이벤트 구독!
        if (AnimHandler != null)
        {
            AnimHandler.OnAttackTriggered += HandleTriggerAttack; // 자식 클래스(Melee, Ranged)에서 구현한 그 공격!
            AnimHandler.OnAnimationFinished += HandleAnimationFinishTrigger;
        }

        InitializeStateMachine();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Health != null)
        {
            Health.OnTakeDamage -= HandleTakeDamage;
            Health.OnDeath -= HandleDeath;
        }
        if (AnimHandler != null)
        {
            AnimHandler.OnAttackTriggered -= HandleTriggerAttack;
            AnimHandler.OnAnimationFinished -= HandleAnimationFinishTrigger;
        }
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
    public bool IsLedgeDetected() => Physics2D.Raycast(ledgeCheck.position, Vector2.down, 1f, Movement.GroundLayer);

    public bool IsPlayerInSight()
    {
        if (Data == null) return false;
        if (Movement == null) return false;

        // 벽(groundLayer)과 플레이어(playerLayer)를 모두 감지하도록 LayerMask를 합칩니다.
        LayerMask mask = playerLayer | Movement.GroundLayer;
        RaycastHit2D hit = Physics2D.Raycast(playerCheck.position, transform.right, Data.detectionDistance, mask);

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
        if (Data == null) return false;
        if (Movement == null) return false;

        LayerMask mask = playerLayer | Movement.GroundLayer;
        RaycastHit2D hit = Physics2D.Raycast(playerCheck.position, transform.right, Data.attackDistance, mask);

        if (hit.collider != null && (playerLayer.value & (1 << hit.collider.gameObject.layer)) > 0)
        {
            return true;
        }
        return false;
    }

    // ==========================================
    // 피격 및 사망 로직 (이벤트 리스너)
    // ==========================================
    private void HandleTakeDamage()
    {
        if (stateMachine.CurrentState == DeadState) return;

        // 역경직 및 카메라 흔들림 (정상 작동)
        if (FeedbackManager.Instance != null && !Health.IsDead)
        {
            FeedbackManager.Instance.TriggerHitStop(0.05f);
            FeedbackManager.Instance.TriggerCameraShake(0.5f);
        }

        // 이미 피격 상태(HurtState)가 아닐 때만 상태를 전환합니다!
        if (stateMachine.CurrentState != HurtState)
        {
            stateMachine.ChangeState(HurtState);
        }
        else
        {
            // [선택 사항] 만약 연타를 맞을 때마다 몬스터가 계속 움찔거리게(Stun-lock) 만들고 싶다면,
            // 상태를 바꾸는 대신 애니메이션만 0프레임부터 강제 재시작하게 할 수 있습니다.
            // Anim.Play("Hurt", -1, 0f); 
        }
    }

    private void HandleDeath()
    {
        // 체력이 다 닳았다면 사망 상태로 강제 전환!
        stateMachine.ChangeState(DeadState);
    }

    // ==========================================
    // 애니메이션 이벤트 전용 브릿지 함수들
    // ==========================================

    // 공격 애니메이션의 '타격 프레임'에 호출할 함수
    public abstract void HandleTriggerAttack();

    // 공격 애니메이션의 '마지막 프레임'에 호출할 함수
    public void HandleAnimationFinishTrigger()
    {
        stateMachine.CurrentState?.AnimationFinishTrigger();
    }

    // 에디터에서 센서 시각화

    private void OnDrawGizmos()
    {
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.blue; // 낭떠러지 감지선은 파란색
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * 1f);
        }

        if (playerCheck != null && Data != null)
        {
            Gizmos.color = IsPlayerInSight() ? Color.green : Color.red; // 플레이어 감지
            Gizmos.DrawLine(playerCheck.position, playerCheck.position + transform.right * Data.detectionDistance);

            Gizmos.color = IsPlayerInAttackRange() ? Color.green : Color.red; // 플레이어가 '공격 사거리' 내에 있는가?
            Gizmos.DrawLine(playerCheck.position, playerCheck.position + transform.right * Data.attackDistance);
        }
    }
}
