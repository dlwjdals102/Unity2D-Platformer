using UnityEngine;

public abstract class AttackActionSO : ScriptableObject
{
    /// <summary>
    /// 공격을 실행하는 추상 메서드.
    /// </summary>
    /// <param name="attacker">공격을 실행하는 주체 (플레이어 또는 몬스터)</param>
    /// <param name="combat">주체의 전투 모듈 (물리 판정 및 투사체 발사 담당)</param>
    /// <param name="damage">최종 데미지 (페이즈 변화나 버프가 적용된 수치)</param>
    public abstract void Execute(Entity attacker, CombatComponent combat, float damage);
}