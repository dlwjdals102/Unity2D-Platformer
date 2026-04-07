using UnityEngine;

[CreateAssetMenu(fileName = "NewRangedAttack", menuName = "Combat/Actions/Ranged Attack")]
public class RangedAttackSO : AttackActionSO
{
    [Header("Ranged Specifics")]
    [Tooltip("ObjectPoolManager에서 꺼내올 투사체의 태그 이름입니다.")]
    public string projectileTag = "Arrow";

    public override void Execute(Entity attacker, CombatComponent combat, float damage)
    {
        // 공격 주체가 바라보는 방향을 계산합니다.
        Vector2 shootDirection = new Vector2(attacker.Movement.FacingDirection, 0);

        // CombatComponent의 투사체 발사 로직을 호출합니다.
        combat.FireProjectile(projectileTag, shootDirection, damage);
    }
}