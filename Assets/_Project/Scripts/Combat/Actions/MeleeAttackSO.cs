using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAttack", menuName = "Combat/Actions/Melee Attack")]
public class MeleeAttackSO : AttackActionSO
{
    public override void Execute(Entity attacker, CombatComponent combat, float damage)
    {
        combat.PerformMeleeAttack(damage);
    }
}
