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
                damageable.TakeDamage(damage, transform);

                // 1. 역경직 및 카메라 흔들림
                /*if (FeedbackManager.Instance != null)
                {
                    FeedbackManager.Instance.TriggerHitStop(0.05f);
                    FeedbackManager.Instance.TriggerCameraShake(0.5f);
                }*/

                // 2. 타격 이펙트 소환!
                if (ObjectPoolManager.Instance != null)
                {
                    // 대상(상자, 보스, 문 등)의 '물리 충돌체'의 정중앙 좌표를 유연하게 가져옵니다.
                    Vector2 hitPosition = target.bounds.center;
                    ObjectPoolManager.Instance.SpawnFromPool("HitEffect", hitPosition, Quaternion.identity);
                }

                // 타격 효과음 재생! ("MeleeHit" 이라는 이름의 소리를 재생해 줘!)
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play("MeleeHit");
                }
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
                    projectile.Setup(direction, damage, targetLayer);
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
