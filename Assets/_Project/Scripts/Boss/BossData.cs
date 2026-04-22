using System.Collections.Generic;
using System;
using UnityEngine;

public enum BehaviorType
{
    NormalMelee,    // 제자리 근접 타격
    ShockwaveSlam   // 제자리 파동 발사 (장풍)
}

// ==========================================
// 1. 보스 공격 패턴 단위 구조체
// ==========================================
[Serializable]
public struct BossAttackInfo
{
    [Tooltip("기획자 식별용 이름 (예: 기본 베기, 충격파)")]
    public string attackName;

    [Tooltip("애니메이터에 쏠 Trigger 파라미터 이름 (예: Attack_Normal, Attack_Slam)")]
    public string animTriggerName;

    [Tooltip("이 공격 패턴의 고유 데미지")]
    public float attackDamage;

    [Tooltip("이 공격 패턴이 닿는 최대 사거리")]
    public float attackDistance;

    [Range(1f, 10f)]
    [Tooltip("랜덤 선택 가중치 (값이 높을수록 전투 중 더 자주 사용하게 됩니다)")]
    public float weight;

    [Header("Behavior Settings")]
    public BehaviorType behaviorType;   // 행동 명찰
    [Tooltip("Ranged 타입일 경우, ObjectPool에서 꺼내올 투사체의 이름(Tag)")]
    public string projectileTag;

    // ==========================================
    // 커스텀 타격 범위 (히트박스) 설정
    // ==========================================
    [Header("Hitbox Settings")]
    [Tooltip("타격 범위 (반지름 넓이)")]
    public float hitRadius;

    [Tooltip("보스의 몸 중심(Pivot)으로부터 타격점이 얼마나 떨어져 있는지 (X: 앞뒤, Y: 위아래)")]
    public Vector2 hitOffset;
}

// ==========================================
// 2. 보스 데이터 클래스 (EnemyData 상속)
// ==========================================
[CreateAssetMenu(fileName = "New Boss Data", menuName = "Data/Boss Data")]
public class BossData : EntityData
{
    [Header("Boss Display Settings")]
    public string bossName = "";

    [Header("Boss AI Settings")]
    [Tooltip("보스가 플레이어를 쫓아갈 때의 이동 속도")]
    public float chaseSpeed = 4f;

    [Tooltip("공격을 마친 후 다음 패턴을 고르기 전까지의 휴식 시간")]
    public float attackCooldown = 2f;

    [Header("Boss Attack Patterns")]
    [Tooltip("이 페이즈에서 보스가 사용할 수 있는 공격 패턴들의 목록입니다.")]
    public List<BossAttackInfo> bossAttacks = new List<BossAttackInfo>();
}