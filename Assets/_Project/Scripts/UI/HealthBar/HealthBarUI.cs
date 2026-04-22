using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : GaugeBarUI<HealthComponent>
{
    // 기존의 SetTarget(Entity) 시그니처를 유지하여 하위 호환성을 보장합니다.
    public virtual void SetTarget(Entity newTarget)
    {
        if (targetComponent != null)
            targetComponent.OnHealthChanged -= UpdateVisuals;

        if (newTarget != null)
        {
            targetComponent = newTarget.Health;
            targetComponent.OnHealthChanged += UpdateVisuals;
            UpdateVisuals(targetComponent.CurrentHealth, targetComponent.MaxHealth);
        }
    }

    protected virtual void OnEnable()
    {
        if (targetComponent != null)
            targetComponent.OnHealthChanged += UpdateVisuals;
    }

    protected virtual void OnDisable()
    {
        if (targetComponent != null)
            targetComponent.OnHealthChanged -= UpdateVisuals;
    }
}