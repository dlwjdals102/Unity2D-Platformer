using UnityEngine;
using UnityEngine.UI;

public abstract class GaugeBarUI<T> : MonoBehaviour where T : Component
{
    [Header("UI Components")]
    public RectTransform container;
    public Image fillDelay;    // 지연 감소용 잔상 (Red/White)
    public Image fillMain;     // 현재 수치용 메인 바 (Green/Blue)

    [Header("Settings")]
    public float pixelsPerUnit = 2f;    // 단위당 픽셀 길이
    public float catchupSpeed = 5f;     // 잔상이 따라오는 속도
    public bool isFixedSize = false;    // 고정 크기 여부

    protected T targetComponent;

    protected virtual void Update()
    {
        if (fillDelay == null || fillMain == null) return;

        // 잔상이 메인 바를 부드럽게 따라가는 공통 연출 로직
        if (fillDelay.fillAmount > fillMain.fillAmount)
            fillDelay.fillAmount = Mathf.Lerp(fillDelay.fillAmount, fillMain.fillAmount, Time.deltaTime * catchupSpeed);
        else
            fillDelay.fillAmount = fillMain.fillAmount;
    }

    // 게이지의 물리적 길이와 메인 바의 fillAmount를 갱신하는 공통 함수
    protected void UpdateVisuals(float current, float max)
    {
        if (!isFixedSize && container != null)
        {
            float targetWidth = max * pixelsPerUnit;
            container.sizeDelta = new Vector2(targetWidth, container.sizeDelta.y);
        }

        if (fillMain != null)
        {
            fillMain.fillAmount = current / max;
        }
    }
}