using UnityEngine;

public class CombatComponent : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private Transform attackPoint; // 근/원거리 공통 사용!
    [SerializeField] private LayerMask targetLayer; // 누구를 때릴 것인가?

    [Header("Melee Settings")]
    [SerializeField] private float meleeRadius = 0.5f; // 근접 공격에만 사용되는 범위

    [Header("Ranged Attack Settings")]
    [SerializeField] private Transform projectileFirePoint;

    // ==========================================
    // 1. 근접 공격 (Melee)
    // ==========================================
    public void PerformMeleeAttack(float damage)
    {
        if (attackPoint == null) return;

        // 설정된 반경 내의 모든 타겟을 감지합니다.
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, meleeRadius, targetLayer);

        foreach (Collider2D target in hitTargets)
        {
            // 대상이 IDamageable(피격 가능한 체력 컴포넌트)을 가지고 있는지 확인
            IDamageable damageable = target.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                // 타격 성공 시 역경직/카메라 쉐이크를 여기서 호출해도 좋습니다 (공격자 기준의 손맛)
            }
        }
    }

    // ==========================================
    // 2. 원거리 공격 (Ranged)
    // ==========================================
    public void FireProjectile(string poolTag, Vector2 direction, float damage)
    {
        if (projectileFirePoint == null) return;

        // 이전에 만들어둔 ObjectPoolManager를 사용해 투사체 소환
        if (ObjectPoolManager.Instance != null)
        {
            GameObject projectileObj = ObjectPoolManager.Instance.SpawnFromPool(poolTag, projectileFirePoint.position, Quaternion.identity);

            if (projectileObj != null)
            {
                // 투사체 초기화 (Projectile 스크립트에 맞게 연결)
                Projectile projectile = projectileObj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Setup(direction, damage);
                }
            }
        }
    }

    // ==========================================
    // 기즈모 (공격 범위 시각화)
    // ==========================================
    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, meleeRadius);
        }
    }
}
