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
    public EnemyRetreatState RetreatState { get; private set; }

    [Header("Data")]
    [field: SerializeField] public EnemyData Data { get; set; } // 데이터 컨테이너 연결
    private PhaseComponent phaseComponent;

    [Header("Base AI Settings")]
    [HideInInspector] public float lastAttackTime;

    // 감지 센서 세팅 추가!
    [Header("Detection Setup")]
    public LayerMask playerLayer;        // 플레이어를 감지할 레이어
    public Transform ledgeCheck;         // 절벽 감지용 빈 오브젝트 (몬스터 앞쪽 발밑)
    public Transform playerCheck;        // 시야 감지 기준점 (보통 몬스터의 눈 위치)
    public float ledgeCheckDistance = 0.5f;


    // 플레이어의 위치를 기억할 변수
    public Transform PlayerTransform { get; private set; }

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

        // 페이즈 컴포넌트가 있다면 구독
        phaseComponent = GetComponent<PhaseComponent>();
        if (phaseComponent != null)
        {
            phaseComponent.OnPhaseChanged += HandlePhaseChange;
        }

        // 게임이 시작될 때 플레이어를 찾아서 기억해둡니다.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerTransform = playerObj.transform;
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
        if (phaseComponent != null)
        {
            phaseComponent.OnPhaseChanged -= HandlePhaseChange;
        }
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine<Enemy>();
        PatrolState = new EnemyPatrolState(this, stateMachine, "Patrol");
        ChaseState = new EnemyChaseState(this, stateMachine, "Chase");
        AttackState = new EnemyAttackState(this, stateMachine, "Attack");
        HurtState = new EnemyHurtState(this, stateMachine, "Hurt");
        DeadState = new EnemyDeadState(this, stateMachine, "Dead");
        RetreatState = new EnemyRetreatState(this, stateMachine, "Idle");
    }

    private void Start()
    {
        // 명시적으로 1페이즈임을 애니메이터에 선언하여 안전장치를 만듭니다.
        if (Anim != null)
        {
            Anim.SetInteger("Phase", 1);
        }

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
    public bool IsLedgeDetected() => Physics2D.Raycast(ledgeCheck.position, Vector2.down, ledgeCheckDistance, Movement.GroundLayer);
    // 뒤쪽 낭떠러지 감지
    public virtual bool IsLedgeBehindDetected()
    {
        if (ledgeCheck == null) return false;

        // 1. 몬스터 중심축(x)에서 앞쪽 센서까지의 거리를 구합니다.
        float offsetToFront = ledgeCheck.position.x - transform.position.x;

        // 2. 그 거리만큼 반대로(등 뒤로) 이동시킨 좌표를 구합니다.
        Vector2 backLedgePos = new Vector2(transform.position.x - offsetToFront, ledgeCheck.position.y);

        // 3. 등 뒤 좌표에서 아래로 레이캐스트를 쏩니다.
        return Physics2D.Raycast(backLedgePos, Vector2.down, ledgeCheckDistance, Movement.GroundLayer);
    }
    // 뒤쪽 벽 감지
    public virtual bool IsWallBehindDetected()
    {
        // 몬스터 중심에서 등 뒤(바라보는 방향의 반대)로 레이저를 쏴서 벽이 있는지 검사합니다.
        // (1f는 몬스터의 몸집(콜라이더)을 고려한 넉넉한 거리입니다. 필요시 조절하세요)
        return Physics2D.Raycast(transform.position, Vector2.right * -Movement.FacingDirection, Movement.WallCheckDistacne, Movement.GroundLayer);
    }

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

    // 대상을 향해 몸을 홱! 돌리는 스마트한 함수
    public void TurnTowards(Transform target)
    {
        if (target == null) return;

        // 타겟이 내 오른쪽에 있는데, 내가 왼쪽을 보고 있다면? -> 뒤집기!
        if (target.position.x > transform.position.x && Movement.FacingDirection == -1)
        {
            Movement.Flip();
        }
        // 타겟이 내 왼쪽에 있는데, 내가 오른쪽을 보고 있다면? -> 뒤집기!
        else if (target.position.x < transform.position.x && Movement.FacingDirection == 1)
        {
            Movement.Flip();
        }
    }

    // ==========================================
    // 피격 및 사망 로직 (이벤트 리스너)
    // ==========================================
    private void HandleTakeDamage(Transform damageSource)
    {
        if (stateMachine.CurrentState == DeadState) return;

        // 상태 전환 전에 넉백 방향을 계산해서 저장합니다!
        DetermineKnockbackDirection(damageSource);

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

    // 페이즈 전환 수신 (데이터 스왑)
    private void HandlePhaseChange(int phaseIndex, PhaseInfo newPhase)
    {
        // 1. 애니메이터의 "Phase" 파라미터를 갱신합니다. (1페이즈=1, 2페이즈=2...)
        // 이를 통해 애니메이터 내의 Any State 트랜지션이 작동하게 됩니다.
        if (Anim != null)
        {
            Anim.SetInteger("Phase", phaseIndex + 1);
        }

        // 2. SO 데이터 교체 (스탯 및 공격 카드 스왑)
        if (newPhase.newPhaseData != null)
        {
            Data = newPhase.newPhaseData;
        }

        // 3. 피드백 연출 및 상태 인터럽트
        /*if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.TriggerHitStop(0.2f);
            FeedbackManager.Instance.TriggerCameraShake(2.0f);
        }*/

        // 하던 행동을 끊고 강제 피격 (나중에 포효상태로 구현할수있음)
        stateMachine.ChangeState(HurtState);
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
    public void HandleTriggerAttack()
    {
        if (Combat == null || Data == null || Data.basicAttackAction == null) return;

        // 적이 들고 있는 '공격 카드(AttackActionSO)'의 능력을 발동시킵니다.
        Data.basicAttackAction.Execute(this, Combat, Data.attackDamage);
    }

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
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + Vector3.down * ledgeCheckDistance);
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
