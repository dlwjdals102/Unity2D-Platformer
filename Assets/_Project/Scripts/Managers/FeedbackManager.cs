using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;
    private bool isHitStopping = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 컴포넌트 자동 할당
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // 1. 타격 정지 (Hit-Stop)
    public void TriggerHitStop(float duration = 0.05f)
    {
        if (isHitStopping) return;
        StartCoroutine(HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        isHitStopping = true;

        // 완전히 0으로 멈추기보다 0.1 정도로 아주 느리게 흘러가게 하는 것이 타격감이 더 쫀득합니다.
        Time.timeScale = 0.1f;

        // Time.timeScale의 영향을 받지 않는 '현실 시간' 기준으로 대기합니다.
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        isHitStopping = false;
    }

    // 2. 카메라 흔들림 (Camera Shake)
    public void TriggerCameraShake(float force = 1f)
    {
        if (impulseSource != null)
        {
            // 설정된 진동 패턴에 힘을 곱해서 발생시킵니다.
            impulseSource.GenerateImpulseWithForce(force);
        }
    }
}