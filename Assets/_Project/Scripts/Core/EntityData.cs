using UnityEngine;


public class EntityData : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;

    [Header("Base Combat")]
    [Tooltip("이 생명체가 기본적으로 사용할 공격 행동 카드입니다.")]
    public AttackActionSO basicAttackAction;
}