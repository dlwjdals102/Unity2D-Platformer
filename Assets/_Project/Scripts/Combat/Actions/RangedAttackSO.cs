using UnityEngine;

[CreateAssetMenu(fileName = "NewRangedAttack", menuName = "Combat/Actions/Ranged Attack")]
public class RangedAttackSO : AttackActionSO
{
    public string projectileTag = "Arrow";

    public override void Execute(Entity attacker, CombatComponent combat, float damage)
    {
        Vector2 shootDirection = new Vector2(attacker.Movement.FacingDirection, 0);

        combat.FireProjectile(projectileTag, shootDirection, damage);
    }
}