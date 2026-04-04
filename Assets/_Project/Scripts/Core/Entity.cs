using System;
using UnityEngine;

// IDamageable을 여기서 상속받게 하여, 모든 Entity는 피격 가능하도록 강제합니다.
public abstract class Entity : MonoBehaviour, IDamageable
{
    [Header("Core Components")]
    public Rigidbody2D RB { get; private set; }
    public Animator Anim { get; private set; }

    [Header("Core Stats")]
    public float maxHealth = 100f;
    public float currentHealth { get; protected set; }
    public int FacingDirection { get; protected set; } = 1;

    // 체력이 변경될 때마다 발송할 이벤트 (현재 체력, 최대 체력)
    public event Action<float, float> OnHealthChanged;

    // ==========================================
    // 공통 충돌 센서 (환경 감지)
    // ==========================================
    [Header("Collision Info")]
    [SerializeField] protected Transform groundCheck; // 발밑 중앙
    [SerializeField] protected float groundCheckDistance = 0.2f;
    [SerializeField] protected Transform wallCheck;   // 몸 앞쪽 중앙
    [SerializeField] protected float wallCheckDistance = 0.5f;
    [SerializeField] protected LayerMask groundLayer; // 바닥/벽 레이어

    // 자식 클래스들의 Awake에서 base.Awake()로 호출하여 컴포넌트를 초기화합니다.
    protected virtual void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    // --- 공통 헬퍼 함수들 ---
    public virtual void SetVelocityX(float x) => RB.linearVelocityX = x;
    public virtual void SetVelocityY(float y) => RB.linearVelocityY = y;
    public virtual void SetVelocity(float x, float y) { RB.linearVelocityX = x; RB.linearVelocityY = y; }

    public virtual void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    //  자식 클래스들(Player, Enemy)이 체력이 깎일 때마다 호출할 헬퍼 함수
    protected void NotifyHealthChanged()
    {
        // 구독자(UI)가 있다면 현재 체력과 최대 체력을 담아서 방송을 쏩니다!
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ==========================================
    // 공통 센서 함수
    // ==========================================
    public virtual bool IsGrounded() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);
    public abstract void TakeDamage(float damage);
    public virtual void Heal(float amount)
    {
        // 이미 죽은 시체라면 회복할 수 없습니다.
        if (currentHealth <= 0) return;

        currentHealth += amount;

        // 체력이 최대치를 넘지 않도록 고정(Clamp)합니다.
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log($"[회복] 현재 체력: {currentHealth}");

        // 핵심: 체력이 변했으니 UI들에게 다시 방송을 송출합니다!
        // 이 한 줄 덕분에 UI 스크립트를 건드릴 필요가 전혀 없습니다.
        NotifyHealthChanged();
    }

    // ==========================================
    // 공통 기즈모 그리기 (자식 클래스에서도 그릴 게 있다면 override 활용)
    // ==========================================
    protected virtual void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
        if (wallCheck != null)
        {
            Gizmos.color = IsWallDetected() ? Color.green : Color.red;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * wallCheckDistance);
        }
    }
}