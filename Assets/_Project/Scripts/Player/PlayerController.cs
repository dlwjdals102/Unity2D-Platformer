using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    // ==========================================
    // 1. 상태 머신 및 상태 인스턴스
    // ==========================================
    public StateMachine stateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerFallState FallState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }

    // ==========================================
    // 2. 핵심 컴포넌트
    // ==========================================
    [Header("Components")]
    public Rigidbody2D RB { get; private set; }
    public Animator Anim { get; private set; }
    private PlayerInputActions inputActions;

    // ==========================================
    // 3. 캐릭터 스탯 세팅 (기획자 조절용)
    // ==========================================
    [Header("Movement Settings")]
    public float moveSpeed = 8f;

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float jumpBufferTime = 0.2f; // 선입력 유효 시간
    public float coyoteTime = 0.1f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack Settings")]
    public float attackDamage = 10f;
    [SerializeField] private Transform attackPoint; // 타격 중심점 (무기 끝부분 등에 빈 오브젝트로 위치)
    [SerializeField] private float attackRadius = 0.8f; // 타격 반경 (원형)
    [SerializeField] private LayerMask enemyLayer; // 적을 판별할 레이어

    [Header("Combo Attack Settings")]
    public int comboCounter = 1; // 현재 몇 단 공격인지 (1, 2, 3)
    public float comboWindow = 0.5f; // 공격 후 다음 공격을 이어갈 수 있는 유예 시간
    public float lastAttackTime = -100f; // 마지막으로 공격이 끝난 시간

    [Header("Health & Defense Settings")]
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }
    public float iFrameDuration = 1f; // 피격 후 무적 시간 (1초)
    private float iFrameTimer;

    // ==========================================
    // 4. 입력 및 상태 확인 (다른 스크립트에서 읽기 전용)
    // ==========================================
    [Header("Status (Read Only)")]
    public Vector2 MoveInput { get; private set; }
    public bool DashInput { get; private set; }
    public bool IsJumpButtonHeld { get; private set; }  // 점프 버튼을 누르고 있는 상태인지 확인하는 프로퍼티
    public bool IsGrounded { get; private set; }
    public int FacingDirection { get; private set; } = 1;   // 1: 오른쪽, -1: 왼쪽
    public float DefaultGravity { get; private set; }
    public bool AttackInput { get; private set; }

    // 타이머 변수들 (내부 로직용)
    private float jumpBufferTimer;
    private float coyoteTimer;
    private float dashStartTime = -100f;    // 쿨타임 체크용

    // 프로퍼티 (조건문 간소화용)
    public bool HasJumpInputBuffer => jumpBufferTimer > 0;
    public bool CanCoyoteJump => coyoteTimer > 0;
    public bool CanDash => Time.time >= dashStartTime + dashCooldown;
    public float CurrentVelocityY => RB.linearVelocityY;
    public bool IsInvincible => iFrameTimer > 0 || stateMachine.CurrentState == DashState;   // 무적 상태인지 확인하는 프로퍼티

    // ==========================================
    // 5. 유니티 생명주기 및 초기화
    // ==========================================
    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();

        DefaultGravity = RB.gravityScale; // 시작할 때 인스펙터에 설정된 중력값 기억
        currentHealth = maxHealth;

        InitializeStateMachine();
        InitializeInputs();
    }
    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine();
        IdleState = new PlayerIdleState(this, stateMachine, "Idle");
        MoveState = new PlayerMoveState(this, stateMachine, "Move");
        JumpState = new PlayerJumpState(this, stateMachine, "Jump");
        FallState = new PlayerFallState(this, stateMachine, "Fall");
        DashState = new PlayerDashState(this, stateMachine, "Dash");
        AttackState = new PlayerAttackState(this, stateMachine, "Attack");
        HurtState = new PlayerHurtState(this, stateMachine, "Hurt");
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
        // 매 프레임 먼저 바닥에 닿아있는지 센서를 통해 확인합니다.
        CheckGrounded();
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
    private void CheckGrounded()
    {
        // 플레이어 발밑에 가상의 사각형(Box)을 그려서 groundLayer와 겹치는지 확인
        IsGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);
    }
    private void UpdateTimers()
    {
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;

        if (IsGrounded) coyoteTimer = coyoteTime;
        else if (coyoteTimer > 0) coyoteTimer -= Time.deltaTime;

        if (iFrameTimer > 0) iFrameTimer -= Time.deltaTime;
    }
    public void UseJumpBuffer() => jumpBufferTimer = 0f;
    public void UseCoyoteTime() => coyoteTimer = 0f;
    public void UseDash() { DashInput = false; dashStartTime = Time.time; }
    public void UseAttackInput() => AttackInput = false;
    public void SetGravityScale(float scale) => RB.gravityScale = scale;
    public void SetVelocityX(float velocityX) => RB.linearVelocityX = velocityX;
    public void SetVelocityY(float velocityY) => RB.linearVelocityY = velocityY;
    public void SetVelocity(float velocityX, float velocityY)
    {
        RB.linearVelocityX = velocityX;
        RB.linearVelocityY = velocityY;
    }
    // 공통 함수 2: 방향 전환 (Flip)
    public void CheckDirectionToFace(float xInput)
    {
        if (xInput != 0 && xInput != FacingDirection) Flip();
    }

    private void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f); // eulerAngles 대신 Rotate 사용으로 최적화
    }
    public void AnimationFinishTrigger()
    {
        stateMachine.CurrentState.AnimationFinishTrigger();
    }

    // 애니메이션 이벤트에서 타격 프레임에 호출할 함수
    public void TriggerAttack()
    {
        // 1. 타격 반경 내의 모든 적(enemyLayer) 콜라이더를 찾아 배열로 반환합니다.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        // 2. 찾은 적들에게 데미지를 입힙니다.
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            // 충돌한 객체가 IDamageable 인터페이스를 가지고 있는지 확인 (가장 안전하고 세련된 방식)
            IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"적 타격 성공! 데미지: {attackDamage}");

                // TODO: 나중에 여기에 타격 이펙트(Particle)나 카메라 쉐이크(Camera Shake) 호출 로직을 추가하면 됩니다.
            }
        }
    }

    // ==========================================
    // IDamageable 인터페이스 구현 (핵심 로직)
    // ==========================================
    public void TakeDamage(float damage)
    {
        // 1. 무적 상태이거나 이미 죽었으면 데미지 무시 (대시 상태 무적은 나중에 여기에 추가)
        if (IsInvincible || currentHealth <= 0) return;

        // 2. 체력 차감
        currentHealth -= damage;
        Debug.Log($"[플레이어 피격] 윽! 남은 체력: {currentHealth}/{maxHealth}");

        // 3. 무적 시간 부여 (다단 히트 방지)
        iFrameTimer = iFrameDuration;

        // 4. 사망 체크 or 피격 상태로 강제 전환
        if (currentHealth <= 0)
        {
            // Die(); // 나중에 사망 로직(PlayerDeadState)으로 교체
            Debug.Log("플레이어 사망...");
        }
        else
        {
            // 이 부분이 FSM의 '강제 인터럽트(Interrupt)' 입니다!
            // 공격 중이든 점프 중이든 모든 것을 끊고 피격 상태로 들어갑니다.
            stateMachine.ChangeState(HurtState);
        }
    }

    // 에디터에서 센서 범위를 시각적으로 보여주는 기능
    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            // 땅에 닿았으면 초록색, 아니면 빨간색으로 박스를 그립니다.
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }

        // 바닥 감지 기즈모 로직 밑에 추가
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
