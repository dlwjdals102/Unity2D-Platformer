using TMPro;
using UnityEngine;

public class BossHealthBarUI : HealthBarUI
{
    [Header("Boss Specific UI")]
    [Tooltip("보스의 이름을 띄워줄 Text UI를 연결하세요")]
    public TextMeshProUGUI bossNameText;

    public void SetBossTarget(Entity bossEntity, string bossName)
    {
        base.SetTarget(bossEntity);

        if (bossNameText != null) bossNameText.text = bossName;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (fillDelay != null) fillDelay.fillAmount = 0;
        if (fillMain != null) fillMain.fillAmount = 0;
    }
}
