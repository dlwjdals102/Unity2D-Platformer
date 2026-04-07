using UnityEngine;

public interface IDamageable
{
    // 데미지 수치와 함께 '공격의 출처'를 넘겨받도록 강제합니다.
    void TakeDamage(float damage, Transform damageSource);
}