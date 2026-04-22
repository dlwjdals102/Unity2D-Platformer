using System;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(MovementComponent))]
[RequireComponent(typeof(CombatComponent))]
public abstract class Entity : MonoBehaviour
{
    [Header("Core Components")]
    public Animator Anim { get; private set; }
    public AnimationEventHandler AnimHandler { get; private set; }
    public HealthComponent Health { get; private set; }
    public MovementComponent Movement { get; private set; }
    public CombatComponent Combat { get; private set; }

    // 넉백 방향을 기억할 변수 (1 = 오른쪽으로 튕김, -1 = 왼쪽으로 튕김)
    public int KnockbackDirection { get; private set; } = 1;
    public float knockbackForceX { get; private set; } = 1f;
    public float knockbackForceY { get; private set; } = 1f;

    // 자식 클래스들의 Awake에서 base.Awake()로 호출하여 컴포넌트를 초기화합니다.
    protected virtual void Awake()
    {
        Anim = GetComponentInChildren<Animator>(); // 나중에 자식으로 분리되면 InChildren 사용!
        AnimHandler = GetComponentInChildren<AnimationEventHandler>();

        Health = GetComponent<HealthComponent>();
        Movement = GetComponent<MovementComponent>();
        Combat = GetComponent<CombatComponent>();
    }

    protected virtual void Update()
    {
        if (Anim != null && Movement.RB != null)
        {
            // 실제 물리 엔진의 X축 속도의 절대값(0~...)을 애니메이터의 Float 파라미터로 계속 넘겨줍니다.
            Anim.SetFloat("xVelocity", Mathf.Abs(Movement.RB.linearVelocity.x));
        }
    }

    protected virtual void OnDestroy()
    {
        // 메모리 누수 방지 (자식들이 override해서 구독 해제용으로 씁니다)
    }

    // 공격자의 위치를 바탕으로 넉백 방향을 계산합니다.
    public void DetermineKnockbackDirection(Transform damageSource)
    {
        if (damageSource != null)
        {
            // 공격자가 내 오른쪽에 있으면 나는 왼쪽(-1)으로 튕겨야 함!
            KnockbackDirection = transform.position.x < damageSource.position.x ? -1 : 1;
        }
        else
        {
            // 만약 독 데미지처럼 출처가 명확하지 않다면, 그냥 내가 바라보는 반대 방향으로 튕김
            KnockbackDirection = -Movement.FacingDirection;
        }
    }
}