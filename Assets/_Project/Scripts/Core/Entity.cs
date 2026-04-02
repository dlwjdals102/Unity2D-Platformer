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

    public abstract void TakeDamage(float damage); // 데미지 처리는 각자 다를 수 있음
}