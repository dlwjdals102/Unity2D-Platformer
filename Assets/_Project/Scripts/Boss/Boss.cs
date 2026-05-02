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
    public BossIdleState IdleState { get; private set; }
    public BossChaseState ChaseState { get; private set; }
    public BossDeadState DeadState { get; private set; }

    [Header("Detection Setup")]
    public LayerMask playerLayer;
    public Transform playerCheck;

    /// <summary>
    /// 플레이어의 Transform 캐시.
    /// GameManager.OnPlayerReady 이벤트로 자동 갱신되므로 매 프레임 Find 호출 불필요.
    /// </summary>
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
        DeadState = new BossDeadState(this, stateMachine, "Dead");
    }

    private void Start()
    {
        // SO 데이터 주입 및 초기화 (무적 시간 없음)
        if (Data != null)
        {
            Health.Initialize(Data.maxHealth);
        }

        // 이미 보스가 처치된 상태인지 확인
        // (DataManager와 sessionData 모두 null 가드)
        if (DataManager.Instance != null
            && DataManager.Instance.sessionData != null
            && DataManager.Instance.sessionData.isBossDefeated)
        {
            stateMachine.ChangeState(DeadState);
            return;
        }

        // GameManager가 이미 플레이어를 알고 있다면 즉시 캐시
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            PlayerTransform = GameManager.Instance.player.transform;
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
                return;
            }
        }
    }

    // ==========================================
    // 이벤트 구독 / 해제
    // ==========================================
    protected virtual void OnEnable()
    {
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

        PhaseComponent phaseComp = GetComponent<PhaseComponent>();
        if (phaseComp != null) phaseComp.OnPhaseChanged += HandlePhaseChange;

        // GameManager 이벤트 구독 (플레이어 캐싱용)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady += HandlePlayerReady;
        }
    }

    protected virtual void OnDisable()
    {
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

        PhaseComponent phaseComp = GetComponent<PhaseComponent>();
        if (phaseComp != null) phaseComp.OnPhaseChanged -= HandlePhaseChange;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady -= HandlePlayerReady;
        }
    }

    /// <summary>
    /// GameManager가 새 씬에서 플레이어를 준비 완료했을 때 호출됩니다.
    /// </summary>
    private void HandlePlayerReady(PlayerController newPlayer)
    {
        PlayerTransform = newPlayer != null ? newPlayer.transform : null;
    }

    private void HandleTakeDamage(Transform damageSource)
    {
        // 보스는 슈퍼아머: 넉백/Hurt 상태 전환 없음
        // 추후 피격 이펙트나 데미지 텍스트는 여기서 처리
    }

    private void HandleDeath()
    {
        // 사망 시 사망 상태로 넘깁니다.
        stateMachine.ChangeState(DeadState);
    }

    private void HandlePhaseChange(int phaseIndex, PhaseInfo info)
    {
        // 1. PhaseComponent에서 받은 새 데이터로 통째로 교체
        if (info.newPhaseData is BossData newBossData)
        {
            Data = newBossData;
        }

        // 2. 페이즈 전환 시 IntroState 재활용으로 연출
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
