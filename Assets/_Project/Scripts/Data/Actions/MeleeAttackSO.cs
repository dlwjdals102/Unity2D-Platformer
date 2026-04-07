using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAttack", menuName = "Combat/Actions/Melee Attack")]
public class MeleeAttackSO : AttackActionSO
{
    public override void Execute(Entity attacker, CombatComponent combat, float damage)
    {
        // CombatComponent의 근접 타격 로직을 그대로 호출합니다.
        combat.PerformMeleeAttack(damage);
    }
}
