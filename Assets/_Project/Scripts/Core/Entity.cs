using System;
using UnityEngine;


[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(MovementComponent))]
public abstract class Entity : MonoBehaviour
{
    [Header("Core Components")]
    public Animator Anim { get; private set; }
    public AnimationEventHandler AnimHandler { get; private set; }
    public HealthComponent Health { get; private set; }
    public MovementComponent Movement { get; private set; }
    public CombatComponent Combat { get; private set; }


    // 자식 클래스들의 Awake에서 base.Awake()로 호출하여 컴포넌트를 초기화합니다.
    protected virtual void Awake()
    {
        Anim = GetComponent<Animator>(); // 나중에 자식으로 분리되면 InChildren 사용!
        AnimHandler = GetComponent<AnimationEventHandler>();
        Health = GetComponent<HealthComponent>();
        Movement = GetComponent<MovementComponent>();
        Combat = GetComponent<CombatComponent>();
    }

    protected virtual void OnDestroy()
    {
        // 메모리 누수 방지 (자식들이 override해서 구독 해제용으로 씁니다)
    }
}