using UnityEngine;
using UnityEngine.Rendering;

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
    public bool PerformMeleeAttack(float damage, float facingDirection = 1f, string hitSpark = "")
    {
        if (attackPoint == null) return false;

        bool isHitSuccess = false; // 타격 성공 여부

        // 설정된 반경 내의 모든 타겟을 감지합니다.
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, meleeRadius, targetLayer);

        foreach (Collider2D target in hitTargets)
        {
            // 대상이 IDamageable(피격 가능한 체력 컴포넌트)을 가지고 있는지 확인
            IDamageable damageable = target.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform);
                isHitSuccess = true;

                // 타겟의 콜라이더 표면 중, 공격 중심점에서 가장 가까운 좌표를 찾습니다!
                Vector2 exactHitPoint = target.ClosestPoint(attackPoint.position);

                // TODO  
                // 찾아낸 정확한 좌표에 스파크를 개별적으로 터뜨립니다. 
                if (!string.IsNullOrWhiteSpace(hitSpark))
                {
                    FeedbackManager.Instance.SpawnVFX(hitSpark, exactHitPoint, facingDirection);
                }
                
            }
        }

        return isHitSuccess;
    }

    // ==========================================
    // 2. 원거리 공격 (Ranged)
    // ==========================================
    public void FireProjectile(string poolTag, Vector2 direction, float damage, float shakeIntensity = 0f, float hitStopDuration = 0f)
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
                    projectile.Setup(direction, damage, targetLayer, shakeIntensity, hitStopDuration);
                }
            }
        }
    }

    // ==========================================
    // 3. 보스 전용 커스텀 근접 공격 (오버로딩)
    // ==========================================
    public bool PerformCustomMeleeAttack(float damage, Vector2 offset, float customRadius, float facingDirection = 1f)
    {
        bool isHitSuccess = false;

        // 보스의 현재 위치(transform.position)를 기준으로, 바라보는 방향에 맞춰 오프셋을 계산합니다.
        Vector2 actualHitPosition = (Vector2)transform.position + new Vector2(offset.x * facingDirection, offset.y);

        // 지정된 위치와 지정된 크기로 타격 판정을 생성합니다!
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(actualHitPosition, customRadius, targetLayer);

        foreach (Collider2D target in hitTargets)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform);
                isHitSuccess = true;

                // TODO: 피격 이펙트 (HitSpark) 등 추가 가능
            }
        }

        return isHitSuccess;
    }

    // ==========================================
    // 4. 보스 전용 커스텀 원거리 투사체 발사
    // ==========================================
    public void FireCustomProjectile(string poolTag, Vector2 offset, float facingDirection, float damage)
    {
        if (ObjectPoolManager.Instance == null) return;

        // 1. 기즈모로 세팅한 오프셋을 적용하여 '실제 투사체가 생성될 위치'를 계산합니다.
        Vector2 actualSpawnPosition = (Vector2)transform.position + new Vector2(offset.x * facingDirection, offset.y);

        // 2. 발사 방향 설정 (바라보는 방향의 X축으로 날아갑니다)
        Vector2 fireDirection = new Vector2(facingDirection, 0f);

        // 3. 투사체를 오브젝트 풀에서 꺼냅니다. (이름이 틀리면 안 나오니 태그 주의!)
        GameObject projectileObj = ObjectPoolManager.Instance.SpawnFromPool(poolTag, actualSpawnPosition, Quaternion.identity);

        if (projectileObj != null)
        {
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                // 4. 기존 투사체 Setup 함수 재활용! 
                // (필요시 화면 흔들림(shakeIntensity) 수치도 BossData에 추가해서 넘겨줄 수 있습니다)
                projectile.Setup(fireDirection, damage, targetLayer, 0f, 0f);
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
