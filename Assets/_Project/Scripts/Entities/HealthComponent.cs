using System;
using System.Collections;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    public event Action<float, float> OnHealthChanged; // 체력이 변경될 때마다 발송할 이벤트 (현재 체력, 최대 체력)
    public event Action<Transform> OnTakeDamage; // 피격 이펙트/역경직용
    public event Action OnDeath;

    [Header("Health Stats")]
    public float BaseMaxHealth { get; protected set; }  // SO에서 가져온 기본값
    public float BonusMaxHealth { get; protected set; } // 아이템 등으로 추가된 값
    public float MaxHealth => BaseMaxHealth + BonusMaxHealth;  // 최종 최대 체력은 이 둘의 합으로 계산됩니다 (Read-Only)
    public float CurrentHealth { get; protected set; }

    [Header("Invincibility")]
    private float iFrameDuration;
    private bool isInvincible = false;

    // 외부(PlayerDashState 등)에서 켜고 끌 수 있는 대시 전용 무적 판정
    public bool IsDashInvincible { get; set; } = false;

    public bool IsDead => CurrentHealth <= 0;

    // 초기화할 때 SO에 있는 iFrameDuration 값을 함께 주입받습니다.
    // 적 몬스터처럼 무적 시간이 없는 경우를 위해 기본값을 0f로 줍니다.
    public void Initialize(float initialMaxHealth, float iFrameTime = 0f)
    {
        BaseMaxHealth = initialMaxHealth;
        BonusMaxHealth = 0;
        CurrentHealth = MaxHealth;

        iFrameDuration = iFrameTime; // 주입 완료!

        NotifyHealthChanged();
    }

    public void TakeDamage(float damage, Transform damageSource)
    {
        // 피격 후 무적이거나, 대시 중이거나, 죽었다면 데미지 무시
        if (isInvincible || IsDashInvincible || IsDead) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); // 0 밑으로 떨어지지 않게 고정

        // 체력이 깎였으니 방송 송출
        NotifyHealthChanged(); 

        // 피격 이벤트를 발생시켜 Enemy나 Player가 연출을 실행하도록 함
        OnTakeDamage?.Invoke(damageSource);

        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
        else
        {
            if (iFrameDuration > 0)
                StartCoroutine(HitStopInvincibility());
        }
    }

    public void Heal(float amount)
    {
        // 이미 죽은 시체라면 회복할 수 없습니다.
        if (IsDead) return;

        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth); // 최대 체력 초과 방지

        NotifyHealthChanged(); // 체력이 찼으니 방송 송출
    }

    public void RestoreFullHealth()
    {
        CurrentHealth = MaxHealth;
        NotifyHealthChanged(); // UI에도 즉시 알림
    }

    public void IncreaseMaxHealth(float amount)
    {
        // 원본 SO를 건드리지 않고, 인스턴스의 '보너스 수치'만 늘립니다.
        BonusMaxHealth += amount;

        // 늘어난 만큼 현재 체력도 보너스로 채워줍니다.
        RestoreFullHealth();

        // UI에 합산된 maxHealth(base + bonus)가 전달됩니다.
        NotifyHealthChanged();
    }

    protected void NotifyHealthChanged()
    {
        // 구독자(UI)가 있다면 현재 체력과 최대 체력을 담아서 방송을 쏩니다!
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private IEnumerator HitStopInvincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(iFrameDuration);
        isInvincible = false;
    }
}
