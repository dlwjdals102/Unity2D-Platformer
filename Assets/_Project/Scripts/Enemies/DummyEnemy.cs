using System.Collections;
using UnityEngine;

public class DummyEnemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

    [Header("Components")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        // 시작할 때 체력을 꽉 채우고, 원래 색상을 기억해 둡니다.
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }


    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"[더미] 윽! {damage} 데미지를 받았다! (남은 체력: {currentHealth}/{maxHealth})");

        // 피격 시각 효과: 짧게 빨간색으로 번쩍이기
        StartCoroutine(FlashRedRoutine());

        // 체력이 0 이하가 되면 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 0.1초 동안 빨간색으로 변했다가 돌아오는 간단한 연출
    private IEnumerator FlashRedRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log("[더미] 파괴되었습니다...");
        // TODO: 나중에 여기에 폭발 파티클 생성이나 사망 사운드를 넣으면 됩니다.

        // 일단은 오브젝트를 씬에서 삭제합니다.
        Destroy(gameObject);
    }
}
