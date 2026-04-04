using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged Specifics")]
    public float attackDamage = 10f;
    public GameObject projectilePrefab; // 우리가 만든 Projectile 프리팹 (화살, 파이어볼 등)
    public Transform firePoint;         // 발사 위치 (활 끝, 지팡이 끝)

    // 부모의 추상 메서드 완성: 투사체 소환 및 발사
    public override void PerformAttack()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // 애니메이션의 '활 시위를 놓는 프레임'에서 투사체를 생성합니다.
            GameObject newProj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            Projectile projComponent = newProj.GetComponent<Projectile>();
            if (projComponent != null)
            {
                // 적이 바라보는 방향(FacingDirection)으로 발사!
                Vector2 shootDirection = new Vector2(FacingDirection, 0);
                projComponent.Setup(shootDirection, attackDamage);
            }
        }
    }
}