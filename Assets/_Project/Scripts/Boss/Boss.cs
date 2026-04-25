using UnityEngine;

public class Boss : Entity
{
    [Header("Boss Data")]
    [field: SerializeField] public BossData Data { get; set; }

    // ==========================================
    // 1. 보스 전용 상태 머신
    // ==========================================
    public StateMachine<Boss> stateMachine { get; private set; }

    // 나중에 하나씩 구현해나갈 보스 전용 상태들
    public BossSleepState SleepState { get; private set; }
    public BossIntroState IntroState { get; private set; }
    public BossIdleState IdleState { get; private set; }     // (쿨타임 & 다음 공격 생각)
    public BossChaseState ChaseState { get; private set; }
    public BossMeleeAttackState MeleeAttackState { get; private set; }
    public BossShockwaveState ShockwaveState { get; private set; }
    public BossDeadState DeadState { get; private set; }

    [Header("Detection Setup")]
    public LayerMask playerLayer;
    public Transform playerCheck;
    public Transform PlayerTransform { get; private set; }

    [Header("Combat Settings")]
    [HideInInspector] public BossAttackInfo NextAttack; // 다음에 쓸 공격 패턴 저장소
    [HideInInspector] public float lastAttackTime = -999f;

    protected override void Awake()
    {
        base.Awake(); 

        stateMachine = new StateMachine<Boss>();

        SleepState = new BossSleepState(this, stateMachine, "Sleep");
        IntroState = new BossIntroState(this, stateMachine, "Intro");
        IdleState = new BossIdleState(this, stateMachine, "Idle");
        ChaseState = new BossChaseState(this, stateMachine, "Move");
        MeleeAttackState = new BossMeleeAttackState(this, stateMachine, "attack");
        ShockwaveState = new BossShockwaveState(this, stateMachine, "attack");
        DeadState = new BossDeadState(this, stateMachine, "Dead");
    }

    private void Start()
    {
        // SO 데이터 주입 및 초기화 (무적 시간 없음)
        if (Data != null)
        {
            Health.Initialize(Data.maxHealth);
        }

        // 추가: 씬에 진입했을 때 이미 보스가 처치된 상태인지 확인
        if (DataManager.Instance != null && DataManager.Instance.sessionData.isBossDefeated)
        {
            // AI를 깨우지 않고 즉시 사망 상태로 진입 (애니메이션은 'Dead' 상태로 고정)
            stateMachine.ChangeState(DeadState);
            return;
        }

        // 보스는 태어나자마자 무조건 대기(Sleep) 상태로 시작합니다!
        stateMachine.Initialize(SleepState);
    }

    protected override void Update()
    {
        base.Update();

        stateMachine.CurrentState?.Update();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.CurrentState?.FixedUpdate();
    }

    // ==========================================
    // 가중치 기반 랜덤 공격 패턴 뽑기
    // ==========================================
    public void ChooseNextAttack()
    {
        if (Data.bossAttacks == null || Data.bossAttacks.Count == 0) return;

        float totalWeight = 0;

        foreach (var attack in Data.bossAttacks) 
            totalWeight += attack.weight;

        float randomVal = Random.Range(0, totalWeight);
        float currentWeight = 0;

        foreach (var attack in Data.bossAttacks)
        {
            currentWeight += attack.weight;
            if (randomVal <= currentWeight)
            {
                NextAttack = attack;
                Debug.Log($"[가챠 완료] 주사위: {randomVal:F1} / 총합: {totalWeight} ? 선택됨: {NextAttack.attackName} (트리거: {NextAttack.animTriggerName})");
                return;
            }
        }
    }

    // ==========================================
    // 2. 컴포넌트 이벤트 구독 (기획 반영)
    // ==========================================
    protected virtual void OnEnable()
    {
        Health.OnTakeDamage += HandleTakeDamage;
        Health.OnDeath += HandleDeath;

        if (AnimHandler != null)
        {
            AnimHandler.OnAttackTriggered += HandleTriggerAttack;
            AnimHandler.OnAnimationFinished += HandleAnimationFinishTrigger;
        }

        PhaseComponent phaseComp = GetComponent<PhaseComponent>();
        if (phaseComp != null) phaseComp.OnPhaseChanged += HandlePhaseChange;
    }

    protected virtual void OnDisable()
    {
        Health.OnTakeDamage -= HandleTakeDamage;
        Health.OnDeath -= HandleDeath;

        if (AnimHandler != null)
        {
            AnimHandler.OnAttackTriggered -= HandleTriggerAttack;
            AnimHandler.OnAnimationFinished -= HandleAnimationFinishTrigger;
        }

        PhaseComponent phaseComp = GetComponent<PhaseComponent>();
        if (phaseComp != null) phaseComp.OnPhaseChanged -= HandlePhaseChange;
    }

    private void HandleTakeDamage(Transform damageSource)
    {
        // 보스는 넉백되지 않으며 피격 애니메이션(Hurt)으로 강제 전환되지 않습니다! (슈퍼아머)
        // 만약 피격 시 이펙트를 터뜨리거나 데미지 텍스트를 띄운다면 여기서 처리합니다.

        // (참고: PhaseComponent가 Health를 구독하고 있으므로, 페이즈 전환은 알아서 작동합니다!)
    }

    private void HandleDeath()
    {
        // 사망 시 사망 상태로 넘깁니다.
        stateMachine.ChangeState(DeadState);
    }

    private void HandlePhaseChange(int phaseIndex, PhaseInfo info)
    {
        Debug.Log($"보스 {phaseIndex + 1} 페이즈 진입! 데이터를 스왑합니다.");

        // 1. 디렉터님이 만든 PhaseComponent에서 새 데이터를 받아 통째로 갈아끼웁니다.
        if (info.newPhaseData is BossData newBossData)
        {
            Data = newBossData;
        }

        // 2. 2페이즈 전용 포효(IntroState)를 재활용하여 멋지게 연출!
        stateMachine.ChangeState(IntroState);
    }

    // ==========================================
    // 3. 헬퍼 함수
    // ==========================================
    public void WakeUp() => stateMachine.ChangeState(IntroState);
    public void TurnTowards(Transform target)
    {
        if (target == null) return;
        float dirToTarget = Mathf.Sign(target.position.x - transform.position.x);
        if (dirToTarget != Movement.FacingDirection) Movement.FlipController(dirToTarget);
    }

    public void HandleTriggerAttack() => stateMachine.CurrentState?.TriggerAttack();
    public void HandleAnimationFinishTrigger() => stateMachine.CurrentState?.AnimationFinishTrigger();
    

    private void OnDrawGizmos()
    {
        if (Data == null || Data.bossAttacks == null) return;

        Gizmos.color = Color.magenta;

        // 플레이 중이면 실제 바라보는 방향, 아니면 에디터 기본값(1)
        float facing = Application.isPlaying && Movement != null ? Movement.FacingDirection : 1f;

        // 데이터에 적힌 모든 공격의 타격 범위를 보스 몸 주변에 보라색 선으로 그려줍니다!
        foreach (var attack in Data.bossAttacks)
        {
            if (attack.hitRadius > 0)
            {
                // 보스의 현재 위치 + (오프셋 * 방향)
                Vector2 actualHitPosition = (Vector2)transform.position + new Vector2(attack.hitOffset.x * facing, attack.hitOffset.y);
                Gizmos.DrawWireSphere(actualHitPosition, attack.hitRadius);
            }

            if (attack.attackDistance > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerCheck.position, playerCheck.position + transform.right * attack.attackDistance);
            }
        }
    }
}
