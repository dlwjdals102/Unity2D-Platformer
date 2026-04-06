using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged Specifics")]
    public string projectilePoolTag = "Arrow"; // 풀 매니저에 요청할 태그 이름

    // 부모의 추상 메서드 완성: 투사체 소환 및 발사
    public override void HandleTriggerAttack()
    {
        if (Combat != null && Data != null)
        {
            // 적이 바라보는 방향 계산
            Vector2 shootDirection = new Vector2(Movement.FacingDirection, 0);

            // Combat 모듈을 사용해 투사체 발사 위임!
            Combat.FireProjectile(projectilePoolTag, shootDirection, Data.attackDamage);
        }
    }
}