using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Target")]
    // PlayerController나 Enemy가 아닌 부모 'Entity'를 받습니다! (핵심 범용성)
    [SerializeField] private Entity targetEntity;

    // 내부적으로 체력 컴포넌트만 따로 캐싱해서 사용합니다!
    private HealthComponent targetHealth;

    [Header("UI Components")]
    // 이 컨테이너의 크기를 늘리면 하위 이미지 3개(Background, Red, Green)가 모두 같이 늘어납니다.
    public RectTransform healthBarContainer;

    [Space]
    public Image background; // 배경 (보통 어두운 색)
    public Image fillRed;    // 지연 감소용 붉은색 게이지 (피격 시 천천히 줄어듦)
    public Image fillGreen;  // 현재 체력용 초록색 게이지 (즉시 줄어듦)

    [Header("Settings")]
    public float pixelsPerHealthUnit = 2f;
    public float redCatchupSpeed = 5f; // 붉은색 게이지가 따라잡는 속도

    private void Awake()
    {
        // 1. 할당된 Entity가 있다면, 그 안에서 HealthComponent만 빼옵니다.
        if (targetEntity != null)
        {
            // Entity의 Awake가 먼저 실행되었는지 확신할 수 없으므로 GetComponent로 안전하게 찾습니다.
            targetHealth = targetEntity.GetComponent<HealthComponent>();
        }
    }

    private void Start()
    {
        // 게임 시작 시, 체력바를 꽉 찬 상태로 초기화합니다.
        if (targetHealth != null)
        {
            UpdateHealthBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);

            // 시작 시 붉은색 게이지도 초록색과 동일하게 꽉 찬 상태로 맞춤
            if (targetHealth.MaxHealth > 0)
            {
                fillRed.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;
            }
        }
    }

    // 오브젝트가 활성화될 때 이벤트를 '구독'합니다.
    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateHealthBar;
        }
    }

    // 오브젝트가 비활성화될 때 이벤트를 '구독 취소'합니다. (메모리 누수 방지!)
    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void Update()
    {
        // 붉은색 게이지가 초록색 게이지보다 많이 남아있다면 (즉, 데미지를 입었다면)
        if (fillRed.fillAmount > fillGreen.fillAmount)
        {
            // 시간을 두고 부드럽게 초록색 게이지 위치까지 깎여나가는 연출
            fillRed.fillAmount = Mathf.Lerp(fillRed.fillAmount, fillGreen.fillAmount, Time.deltaTime * redCatchupSpeed);
        }
        else
        {
            // 회복할 때는 붉은색 게이지가 딜레이 없이 즉시 초록색을 따라감
            fillRed.fillAmount = fillGreen.fillAmount;
        }
    }

    // 이벤트가 수신되면 실행될 실제 UI 업데이트 로직
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // 1. 최대 체력(max)이 변하면 부모 컨테이너의 가로 길이 전체를 늘립니다.
        // (자식 이미지들의 앵커가 Stretch로 되어있다면 3장 모두 자동으로 늘어납니다!)
        if (healthBarContainer != null)
        {
            float targetWidth = maxHealth * pixelsPerHealthUnit;
            healthBarContainer.sizeDelta = new Vector2(targetWidth, healthBarContainer.sizeDelta.y);
        }

        // 2. 현재 체력 비율을 초록색 게이지에 즉시 반영합니다.
        if (maxHealth > 0)
        {
            fillGreen.fillAmount = currentHealth / maxHealth;
        }
    }
}