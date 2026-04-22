using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PhaseInfo
{
    [Header("Phase Condition")]
    [Range(0f, 1f)]
    [Tooltip("현재 체력이 이 비율(%) 이하로 떨어지면 페이즈가 넘어갑니다.")]
    public float healthThresholdRatio;

    [Header("Phase Data Swap")]
    [Tooltip("페이즈 전환 시 몬스터의 스탯과 공격 패턴을 통째로 바꿀 새 데이터입니다.")]
    public EntityData newPhaseData;

    [Header("Phase Events")]
    [Tooltip("에디터에서 파티클 재생, 카메라 흔들림 등을 연결할 수 있습니다.")]
    public UnityEvent OnPhaseEnter;
}

[RequireComponent(typeof(HealthComponent))]
public class PhaseComponent : MonoBehaviour
{
    // C# 스크립트(Enemy 등)가 구독할 페이즈 변경 이벤트
    public event Action<int, PhaseInfo> OnPhaseChanged;

    [Header("Phases (Phase 2부터 순서대로 등록)")]
    [SerializeField] private List<PhaseInfo> phases;

    private HealthComponent health;
    private int currentPhaseIndex = 0;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        health.OnHealthChanged += CheckPhase;
    }

    private void CheckPhase(float currentHealth, float maxHealth)
    {
        // 체력이 0이거나 세팅된 페이즈를 다 소모했다면 무시
        if (health.IsDead || phases == null || currentPhaseIndex >= phases.Count) return;

        float ratio = currentHealth / maxHealth;

        // 설정된 체력 비율 이하로 떨어졌다면 페이즈 전환!
        if (ratio <= phases[currentPhaseIndex].healthThresholdRatio && currentHealth > 0)
        {
            TriggerNextPhase();
        }
    }

    private void TriggerNextPhase()
    {
        PhaseInfo triggeredPhase = phases[currentPhaseIndex];

        // 인덱스 증가(0에서 시작하므로 +1 한 값이 실제 페이즈 번호가 됩니다)
        currentPhaseIndex++;

        Debug.Log($"페이즈 {currentPhaseIndex + 1} 돌입!");

        // 1. 유니티 인스펙터에 연결된 이벤트 실행 (이펙트, 사운드 등)
        triggeredPhase.OnPhaseEnter?.Invoke();

        // 2. C# 스크립트들에게 페이즈 변경 사실과 새로운 데이터를 방송
        OnPhaseChanged?.Invoke(currentPhaseIndex, triggeredPhase);
    }

    private void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= CheckPhase;
    }
}