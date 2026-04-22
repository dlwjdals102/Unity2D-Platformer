using UnityEngine;

public class ManaBarUI : GaugeBarUI<ManaComponent>
{
    public virtual void SetTarget(ManaComponent newMana)
    {
        if (targetComponent != null)
            targetComponent.OnManaChanged -= UpdateVisuals;

        targetComponent = newMana;

        if (targetComponent != null)
        {
            targetComponent.OnManaChanged += UpdateVisuals;
            UpdateVisuals(targetComponent.CurrentMana, targetComponent.MaxMana);
        }
    }

    protected virtual void OnEnable()
    {
        if (targetComponent != null)
            targetComponent.OnManaChanged += UpdateVisuals;
    }

    protected virtual void OnDisable()
    {
        if (targetComponent != null)
            targetComponent.OnManaChanged -= UpdateVisuals;
    }
}
