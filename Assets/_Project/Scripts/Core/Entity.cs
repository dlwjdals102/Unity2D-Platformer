using System;
using UnityEngine;

// IDamageable을 여기서 상속받게 하여, 모든 Entity는 피격 가능하도록 강제합니다.
public abstract class Entity : MonoBehaviour, IDamageable
{
    [Header("Core Components")]
    public Rigidbody2D RB { get; private set; }
    public Animator Anim { get; private set; }

    [Header("Runtime Health Stats")]
    public float BaseMaxHealth { get; protected set; }  // SO에서 가져온 기본값
    public float BonusMaxHealth { get; protected set; } // 아이템 등으로 추가된 값
    // 최종 최대 체력은 이 둘의 합으로 계산됩니다 (Read-Only)
    public float MaxHealth => BaseMaxHealth + BonusMaxHealth;
    public float CurrentHealth { get; protected set; }

    public int FacingDirection { get; protected set; } = 1;
    protected bool facingRight = true;

    // 체력이 변경될 때마다 발송할 이벤트 (현재 체력, 최대 체력)
    public event Action<float, float> OnHealthChanged;

    // ==========================================
    // 공통 충돌 센서 (환경 감지)
    // ==========================================
    [Header("Collision Info")]
    [SerializeField] protected Transform groundCheck; // 발밑 중앙
    [SerializeField] protected float groundCheckDistance = 0.2f;
    [SerializeField] protected LayerMask groundLayer; // 바닥/벽 레이어
    [SerializeField] protected Transform wallCheck;   // 몸 앞쪽 중앙
    [SerializeField] protected float wallCheckDistance = 0.5f;

    // 자식 클래스들의 Awake에서 base.Awake()로 호출하여 컴포넌트를 초기화합니다.
    protected virtual void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
    }

    #region Movement & Physics
    public virtual void SetVelocity(float xVelocity, float yVelocity)
    {
        RB.linearVelocity = new Vector2(xVelocity, yVelocity);
        //FlipController(xVelocity); // 이동 방향에 맞춰 자동으로 뒤집기
    }
    public void ZeroVelocity()
    {
        RB.linearVelocity = Vector2.zero;
    }

    #endregion

    #region Combat & Health

    public virtual void TakeDamage(float damage)
    {
        // 이미 체력이 0 이하면 데미지를 무시합니다.
        if (CurrentHealth <= 0) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); // 0 밑으로 떨어지지 않게 고정

        NotifyHealthChanged(); // 체력이 깎였으니 방송 송출
    }

    public virtual void Heal(float amount)
    {
        // 이미 죽은 시체라면 회복할 수 없습니다.
        if (CurrentHealth <= 0) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); // 최대 체력 초과 방지

        NotifyHealthChanged(); // 체력이 찼으니 방송 송출
    }

    public virtual void RestoreFullHealth()
    {
        CurrentHealth = MaxHealth;
        NotifyHealthChanged(); // UI에도 즉시 알림
    }

    public virtual void IncreaseMaxHealth(float amount)
    {
        // 원본 SO를 건드리지 않고, 인스턴스의 '보너스 수치'만 늘립니다.
        BonusMaxHealth += amount;

        // 늘어난 만큼 현재 체력도 보너스로 채워줍니다.
        RestoreFullHealth();

        // UI에 합산된 maxHealth(base + bonus)가 전달됩니다.
        NotifyHealthChanged();
    }

    protected void NotifyHealthChanged()
    {
        // 구독자(UI)가 있다면 현재 체력과 최대 체력을 담아서 방송을 쏩니다!
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    #endregion

    #region Sensors

    public virtual bool IsGrounded() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);

    #endregion

    #region Flipping

    public virtual void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    public virtual void FlipController(float moveX)
    {
        if (moveX > 0 && !facingRight) Flip();
        else if (moveX < 0 && facingRight) Flip();
    }

    #endregion

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