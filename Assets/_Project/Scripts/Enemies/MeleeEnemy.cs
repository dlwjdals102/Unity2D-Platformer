using UnityEngine;

public class MeleeEnemy : Enemy
{
    // 부모의 추상 메서드 완성: 물리적인 타격 판정 실행
    public override void HandleTriggerAttack()
    {
        if (Combat != null && Data != null)
        {
            // Combat 모듈을 사용해 SO 데이터에 있는 데미지로 근접 타격!
            Combat.PerformMeleeAttack(Data.attackDamage);
        }
    }

}
