using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("Melee Specifics")]
    public float attackDamage = 10f;
    public Transform attackCheck; // 타격 판정 중심점
    public float attackRadius = 0.5f;

    // 부모의 추상 메서드 완성: 물리적인 타격 판정 실행
    public override void PerformAttack()
    {
        // 애니메이션의 '주먹을 뻗는 프레임'에서 실행되어 플레이어에게 데미지를 줍니다.
        Collider2D hit = Physics2D.OverlapCircle(attackCheck.position, attackRadius, playerLayer);

        if (hit != null)
        {
            hit.GetComponent<PlayerController>()?.TakeDamage(attackDamage);
        }
    }

    // 에디터에서 타격 범위를 시각적으로 보여주기 위한 헬퍼 함수
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (attackCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackCheck.position, attackRadius);
        }
    }
}
