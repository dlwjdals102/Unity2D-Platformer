using UnityEngine;

public class PlayerController : Entity
{
    // ==========================================
    // 상태 머신 및 상태 인스턴스
    // ==========================================
    public StateMachine<PlayerController> StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    // ==========================================
    // 캐릭터 스탯 세팅
    // ==========================================
    [Header("Data")]
    [field: SerializeField] public PlayerData Data { get; protected set; } // 데이터 컨테이너 연결
    public ManaComponent Mana { get; private set; } // 마나 컴포넌트 추가

    public float DefaultGravity { get; private set; }
    public float CurrentVelocityY => Movement.RB.linearVelocityY;

    // ==========================================
    // 입력 버퍼(Buffer) 및 조작감(Game Feel) 세팅
    // ==========================================
    [Header("Input Buffer Settings")]
    public float jumpBufferTime = 0.2f;   // 점프 선입력 유예 시간
    public float dashBufferTime = 0.2f;   // 대시 선입력 유예 시간
    public float attackBufferTime = 0.2f; // 공격 선입력 유예 시간
    public float coyoteTime = 0.2f;       // 코요테 타임 (낭떠러지 점프 허용 시간)

    [Tooltip("점프 키를 짧게 눌렀을 때 상승 속도를 깎아내는 배수 (ex: 0.5면 절반으로 깎임)")]
    public float jumpCutMultiplier = 0.5f;

    // 내부 타이머 카운터
    private float jumpBufferCounter;
    private float dashBufferCounter;
    private float attackBufferCounter;
    private float coyoteCounter;

    // 상태 머신에서 읽어갈 핵심 프로퍼티들
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpButtonHeld { get; private set; }  // 점프 버튼을 누르고 있는 상태인지 확인하는 프로퍼티

    public bool HasJumpInputBuffer => jumpBufferCounter > 0;
    public bool HasDashBuffer => dashBufferCounter > 0;
    public bool HasAttackBuffer => attackBufferCounter > 0;
    public bool CanCoyoteJump => coyoteCounter > 0;

    // ==========================================
    // 콤보 및 대시 쿨타임 관리
    // ==========================================
    [Header("Combat & Dash Status")]
    public float comboWindow = 0.5f;
    [HideInInspector] public int comboCounter = 1;
    [HideInInspector] public float lastAttackTime;

    public bool CanDash => Time.time >= lastDashTime + Data.dashCooldown && Mana.CurrentMana >= Data.dashManaCost;
    private float lastDashTime;
    

    // ==========================================
    // 유니티 생명주기 및 초기화
    // ==========================================
    protected override void Awake()
    {
        base.Awake();

        // 마나 컴포넌트 가져오기
        Mana = GetComponent<ManaComponent>();

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        StateMachine = new StateMachine<PlayerController>();
        IdleState = new PlayerIdleState(this, StateMachine, "Idle");
        MoveState = new PlayerMoveState(this, StateMachine, "Move");
        JumpState = new PlayerJumpState(this, StateMachine, "Jump");
        FallState = new PlayerFallState(this, StateMachine, "Fall");
        DashState = new PlayerDashState(this, StateMachine, "Dash");
        AttackState = new PlayerAttackState(this, StateMachine, "Attack");
        HurtState = new PlayerHurtState(this, StateMachine, "Hurt");
        DeadState = new PlayerDeadState(this, StateMachine, "Dead");
    }

    private void Start()
    {
        DefaultGravity = Movement.RB.gravityScale; // 시작할 때 인스펙터에 설정된 중력값 기억

        // SO 데이터 주입
        if (Data != null)
        {
            Health.Initialize(Data.maxHealth, Data.iFrameDuration);
            Mana?.Initialize(Data.maxMana, Data.manaRegenRate);
        }

        // 시작 시 Idle 상태로 초기화
        StateMachine.Initialize(IdleState);
    }

    // ==========================================
    // 메인 업데이트 로직
    // ==========================================
    protected override void Update()
    {
        base.Update();

        UpdateInputAndBuffers();
        UpdateTimers();
        
        StateMachine.CurrentState.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.FixedUpdate();
    }

    // ==========================================
    // 헬퍼 (Helper) 및 유틸리티 함수
    // ==========================================

    private void UpdateInputAndBuffers()
    {
        // 1. 이동 조이스틱/방향키 값 읽기
        MoveInput = InputManager.Instance.Controls.Player.Move.ReadValue<Vector2>();

        // 2. 가변 점프용 버튼 홀드 지속 상태 읽기
        IsJumpButtonHeld = InputManager.Instance.Controls.Player.Jump.IsPressed();

        // 3. 단발성 입력(WasPressedThisFrame) 감지 시 해당 액션의 타이머 꽉 채우기
        if (InputManager.Instance.Controls.Player.Jump.WasPressedThisFrame())
            jumpBufferCounter = jumpBufferTime;

        if (InputManager.Instance.Controls.Player.Dash.WasPressedThisFrame())
            dashBufferCounter = dashBufferTime;

        if (InputManager.Instance.Controls.Player.Attack.WasPressedThisFrame())
            attackBufferCounter = attackBufferTime;
    }

    private void UpdateTimers()
    {
        // 선입력 타이머 실시간 차감
        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;
        if (dashBufferCounter > 0) dashBufferCounter -= Time.deltaTime;
        if (attackBufferCounter > 0) attackBufferCounter -= Time.deltaTime;

        // 코요테 & 대시 쿨타임 로직
        if (Movement.IsGrounded())
        {
            coyoteCounter = coyoteTime; // 땅에 닿아있으면 코요테 타임 지속
        }
        else
        {
            // 공중에 떠 있다면 코요테 타이머 차감 시작
            if (coyoteCounter > 0) coyoteCounter -= Time.deltaTime;
        }
    }

    // ==========================================
    // 버퍼 소모 함수 (각 State 진입 시 호출됨)
    // ==========================================
    public void UseJumpBuffer() => jumpBufferCounter = 0;
    public void UseDashBuffer() => dashBufferCounter = 0;
    public void UseAttackBuffer() => attackBufferCounter = 0;
    public void UseCoyoteTime() => coyoteCounter = 0;
    public void UseDash()
    {
        lastDashTime = Time.time;
    }

    // ==========================================
    // 유틸리티 함수
    // ==========================================

    public void SetDashInvincibility(bool isInvincible)
    {
        if (Health != null) Health.IsDashInvincible = isInvincible;
    }

    public void SetGravityScale(float scale)
    {
        Movement.RB.gravityScale = scale;
    }
    
    protected override void Respawn()
    {
        base.Respawn();

        // 3. 죽음 상태에서 빠져나와 다시 기본 상태(Idle)로 복귀
        StateMachine.ChangeState(IdleState);
    }

    // ==========================================
    // 생명주기 및 이벤트 구독 (Observer Pattern)
    // ==========================================
    private void OnEnable()
    {
        // 체력 및 애니메이션 이벤트 구독
        if (Health != null)
        {
            Health.OnTakeDamage += HandleTakeDamage;
            Health.OnDeath += HandleDeath;
        }
        if (AnimHandler != null)
        {
            AnimHandler.OnAttackTriggered += HandleTriggerAttack;
            AnimHandler.OnAnimationFinished += HandleAnimationFinishTrigger;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy(); // Entity 베이스의 OnDestroy 실행

        // 메모리 누수 방지용 구독 해제
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

    // ==========================================
    // 이벤트 핸들러 로직
    // ==========================================
    private void HandleTakeDamage(Transform damageSource)
    {
        if (StateMachine.CurrentState == DeadState) return;

        // 상태 전환 전에 넉백 방향을 계산해서 저장합니다!
        DetermineKnockbackDirection(damageSource);

        // FSM 강제 인터럽트: 공격 중이든 점프 중이든 모든 것을 끊고 피격 상태로 들어갑니다!
        StateMachine.ChangeState(HurtState);
    }
    private void HandleDeath()
    {
        StateMachine.ChangeState(DeadState);
    }
    // 애니메이션 이벤트에서 타격 프레임에 호출할 함수
    private void HandleTriggerAttack()
    {
        // 현재 내가 무슨 상태(State)이든 상관없이, 그 상태에게 "타격해라!" 라고 명령을 넘김
        StateMachine.CurrentState?.TriggerAttack();
    }
    private void HandleAnimationFinishTrigger()
    {
        StateMachine.CurrentState.AnimationFinishTrigger();
    }


    /// <summary>
    /// 씬 이동 시 매니저가 호출합니다. 플레이어가 자신의 현재 스탯을 상자에 담아 반환합니다.
    /// </summary>
    public DataManager.GameData ExportSessionData()
    {
        DataManager.GameData data = new DataManager.GameData();

        // 플레이어만이 자신의 속주머니(Health, Mana)를 열어 수치를 기입합니다.
        if (Health != null)
        {
            data.currentHealth = Health.CurrentHealth;
            data.maxHealth = Data.maxHealth; // (영구 체력 증가 기믹이 있다면 Health.MaxHealth 등을 사용)
        }

        if (Mana != null)
        {
            data.currentMana = Mana.CurrentMana;
            data.maxMana = Data.maxMana;
        }

        return data;
    }

    /// <summary>
    /// 새 씬 로드 시 GameManager가 호출합니다. 매니저가 던져준 상자를 뜯어 내 몸에 적용합니다.
    /// </summary>
    public void ImportSessionData(DataManager.GameData data)
    {
        Debug.Log($"[Import] 상자에서 꺼낸 값 - HP: {data.currentHealth}, MP: {data.currentMana}");

        if (Health != null)
            Health.LoadSavedHealth(data.currentHealth);

        if (Mana != null)
            Mana.LoadSavedMana(data.currentMana);
    }
}
