using System;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : Entity
{
    // ==========================================
    // 1. 상태 머신 및 상태 인스턴스
    // ==========================================
    public StateMachine<PlayerController> stateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    // ==========================================
    // 2. 핵심 컴포넌트
    // ==========================================
    [Header("Components")]
    private PlayerInputActions inputActions;

    // ==========================================
    // 3. 캐릭터 스탯 세팅 (기획자 조절용)
    // ==========================================
    [Header("Data")]
    [field: SerializeField] public PlayerData Data { get; protected set; } // 데이터 컨테이너 연결

    [Header("Jump Settings")]
    public float jumpBufferTime = 0.2f; // 선입력 유효 시간
    public float coyoteTime = 0.1f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Combo Attack Settings")]
    public int comboCounter = 1; // 현재 몇 단 공격인지 (1, 2, 3)
    public float comboWindow = 0.5f; // 공격 후 다음 공격을 이어갈 수 있는 유예 시간
    public float lastAttackTime = -100f; // 마지막으로 공격이 끝난 시간

    [Header("Health & Defense Settings")]
    private float iFrameTimer;

    // ==========================================
    // 4. 입력 및 상태 확인 (다른 스크립트에서 읽기 전용)
    // ==========================================
    [Header("Status (Read Only)")]
    public Vector2 MoveInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool IsJumpButtonHeld { get; private set; }  // 점프 버튼을 누르고 있는 상태인지 확인하는 프로퍼티
    public float DefaultGravity { get; private set; }
    public bool AttackInput { get; private set; }

    // 타이머 변수들 (내부 로직용)
    private float jumpBufferTimer;
    private float coyoteTimer;
    private float dashStartTime = -100f;    // 쿨타임 체크용

    // 프로퍼티 (조건문 간소화용)
    public bool HasJumpInputBuffer => jumpBufferTimer > 0;
    public bool CanCoyoteJump => coyoteTimer > 0;
    public bool CanDash => Time.time >= dashStartTime + Data.dashCooldown;
    public float CurrentVelocityY => Movement.RB.linearVelocityY;
    public bool IsInvincible => iFrameTimer > 0 || stateMachine.CurrentState == DashState;   // 무적 상태인지 확인하는 프로퍼티

    // ==========================================
    // 5. 유니티 생명주기 및 초기화
    // ==========================================
    protected override void Awake()
    {
        base.Awake();

        DefaultGravity = Movement.RB.gravityScale; // 시작할 때 인스펙터에 설정된 중력값 기억

        // SO 데이터 주입
        if (Data != null)
        {
            Health.Initialize(Data.maxHealth, Data.iFrameDuration);
        }
        // 생존 이벤트 구독
        if (Health != null)
        {
            Health.OnTakeDamage += HandleTakeDamage;
            Health.OnDeath += HandleDeath;
        }
        // 애니메이션 이벤트 구독
        if (AnimHandler != null)
        {
            AnimHandler.OnAttackTriggered += HandleTriggerAttack;
            AnimHandler.OnAnimationFinished += HandleAnimationFinishTrigger;
        }

        InitializeStateMachine();
        InitializeInputs();
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
        stateMachine = new StateMachine<PlayerController>();
        IdleState = new PlayerIdleState(this, stateMachine, "Idle");
        MoveState = new PlayerMoveState(this, stateMachine, "Move");
        JumpState = new PlayerJumpState(this, stateMachine, "Jump");
        FallState = new PlayerFallState(this, stateMachine, "Fall");
        DashState = new PlayerDashState(this, stateMachine, "Dash");
        AttackState = new PlayerAttackState(this, stateMachine, "Attack");
        HurtState = new PlayerHurtState(this, stateMachine, "Hurt");
        DeadState = new PlayerDeadState(this, stateMachine, "Dead");
    }
    private void InitializeInputs()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Jump.started += context => { jumpBufferTimer = jumpBufferTime; IsJumpButtonHeld = true; };
        inputActions.Player.Jump.canceled += context => IsJumpButtonHeld = false;

        inputActions.Player.Dash.started += context => DashInput = true;
        inputActions.Player.Dash.canceled += context => DashInput = false;

        inputActions.Player.Attack.started += context => AttackInput = true;
        inputActions.Player.Attack.canceled += context => AttackInput = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        // 시작 시 Idle 상태로 초기화
        stateMachine.Initialize(IdleState);
    }

    // ==========================================
    // 6. 메인 업데이트 로직
    // ==========================================
    private void Update()
    {
        UpdateTimers();

        // 이동 입력: 폴링(Polling) 방식 (매 프레임 Vector2 값을 읽어옴)
        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
        stateMachine.CurrentState.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.FixedUpdate();
    }

    // ==========================================
    // 7. 헬퍼 (Helper) 및 유틸리티 함수
    // ==========================================
    
    private void UpdateTimers()
    {
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;

        if (Movement.IsGrounded()) coyoteTimer = coyoteTime;
        else if (coyoteTimer > 0) coyoteTimer -= Time.deltaTime;

        if (iFrameTimer > 0) iFrameTimer -= Time.deltaTime;
    }

    public void UseJumpBuffer() => jumpBufferTimer = 0f;
    public void UseCoyoteTime() => coyoteTimer = 0f;
    public void UseDash() { DashInput = false; dashStartTime = Time.time; }
    public void UseAttackInput() => AttackInput = false;
    public void SetGravityScale(float scale) => Movement.RB.gravityScale = scale;
    
    public void Respawn()
    {
        // 1. 물리력 초기화 (떨어지던 중이었다면 멈춤)
        Movement.ZeroVelocity();

        // 2. 체력을 다시 꽉 채움 (Entity의 Heal 함수 활용)
        Health.RestoreFullHealth();

        // 3. 죽음 상태에서 빠져나와 다시 기본 상태(Idle)로 복귀
        stateMachine.ChangeState(IdleState);

        Debug.Log("플레이어 부활 완료!");
    }
    // 대시 스크립트(PlayerDashState)에서 사용할 수 있도록 무적 온오프 헬퍼 함수 추가
    public void SetDashInvincibility(bool isInvincible)
    {
        if (Health != null) Health.IsDashInvincible = isInvincible;
    }

    // ==========================================
    // 8. 이벤트 리스너
    // ==========================================
    private void HandleTakeDamage(Transform damageSource)
    {
        if (stateMachine.CurrentState == DeadState) return;

        // 상태 전환 전에 넉백 방향을 계산해서 저장합니다!
        DetermineKnockbackDirection(damageSource);

        // FSM 강제 인터럽트: 공격 중이든 점프 중이든 모든 것을 끊고 피격 상태로 들어갑니다!
        stateMachine.ChangeState(HurtState);
    }

    private void HandleDeath()
    {
        stateMachine.ChangeState(DeadState);
    }
    // 애니메이션 이벤트에서 타격 프레임에 호출할 함수
    private void HandleTriggerAttack()
    {
        if (Combat != null)
        {
            Combat.PerformMeleeAttack(Data.attackDamage);
            // 디버그 로그나 추가 이펙트는 CombatComponent 내부로 옮기거나 여기서 이벤트를 받아 처리할 수 있습니다.
        }
    }
    private void HandleAnimationFinishTrigger()
    {
        stateMachine.CurrentState.AnimationFinishTrigger();
    }
}
