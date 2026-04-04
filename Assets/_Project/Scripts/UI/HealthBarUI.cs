using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage; // 체력바 채우기 이미지

    [Header("Target")]
    // PlayerController나 Enemy가 아닌 부모 'Entity'를 받습니다! (핵심 범용성)
    [SerializeField] private Entity targetEntity;

    private void Start()
    {
        // 게임 시작 시, 체력바를 꽉 찬 상태로 초기화합니다.
        if (targetEntity != null)
        {
            UpdateHealthBar(targetEntity.currentHealth, targetEntity.maxHealth);
        }
    }

    // 오브젝트가 활성화될 때 이벤트를 '구독'합니다.
    private void OnEnable()
    {
        if (targetEntity != null)
        {
            targetEntity.OnHealthChanged += UpdateHealthBar;
        }
    }

    // 오브젝트가 비활성화될 때 이벤트를 '구독 취소'합니다. (메모리 누수 방지!)
    private void OnDisable()
    {
        if (targetEntity != null)
        {
            targetEntity.OnHealthChanged -= UpdateHealthBar;
        }
    }

    // 이벤트가 수신되면 실행될 실제 UI 업데이트 로직
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // 백분율(0.0 ~ 1.0)을 계산하여 fillAmount에 적용합니다.
        fillImage.fillAmount = currentHealth / maxHealth;
    }
}