using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // 이 스크립트를 넣으면 Rigidbody2D가 자동으로 붙습니다!
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;       // 날아가는 속도
    public float lifeTime = 3f;     // 3초 뒤 자동 소멸 (화면 밖으로 날아간 투사체 정리)

    private Rigidbody2D rb;
    private float actualDamage; // 발사자가 넘겨준 데미지를 기억할 변수
    private LayerMask targetLayer; // 내가 때려야 할 적의 레이어

    // 피드백(타격감) 수치를 기억할 변수 추가
    private float shakeIntensity;
    private float hitStopDuration;

    // Ground 레이어 인덱스 캐싱 (매 충돌마다 LayerMask.NameToLayer 호출 방지)
    private static readonly int GroundLayerId = LayerMask.NameToLayer(Define.LayerNames.Ground);

    // 풀링 시스템에서 같은 프레임에 여러 콜라이더와 부딪쳐도 중복 처리되지 않도록 가드
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // (풀에서 꺼내져 활성화될 때마다 실행됨)
    private void OnEnable()
    {
        // 활성화될 때마다 충돌 가드 초기화
        hasHit = false;

        // lifeTime 뒤에 Deactivate 함수 실행 예약
        Invoke(nameof(Deactivate), lifeTime);
    }

    private void OnDisable()
    {
        // 비활성화될 때 예약된 타이머를 취소합니다. (버그 방지)
        CancelInvoke();
    }

    /// <summary>
    /// 발사자(보스/적)가 투사체를 풀에서 꺼낸 직후 호출하여 초기 상태를 주입합니다.
    /// </summary>
    public void Setup(Vector2 moveDirection, float damageAmount, LayerMask targetMask, float shake = 0f, float hitStop = 0f)
    {
        actualDamage = damageAmount;
        targetLayer = targetMask;
        // 피드백 수치 저장
        shakeIntensity = shake;
        hitStopDuration = hitStop;

        // 방향을 정규화(길이를 1로 만듦)한 뒤 속도를 곱해 날려 보냅니다.
        rb.linearVelocity = moveDirection.normalized * speed;

        // 투사체 이미지(화살촉 등)가 날아가는 방향을 바라보도록 회전시킵니다.
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 투사체가 무언가에 부딪혔을 때의 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 한 번 명중한 투사체는 추가 충돌 무시 (중복 데미지 방지)
        if (hasHit) return;

        // 1. 타겟 레이어와 충돌했는지 확인
        if ((targetLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // 이제 이 투사체는 플레이어, 파괴 가능한 사물, (만약 플레이어가 쏜다면) 몬스터까지 모두 타격 가능합니다.
            IDamageable target = collision.GetComponent<IDamageable>();
            if (target != null)
            {
                hasHit = true;
                // 넉백 방향 계산을 위해 transform(투사체의 위치)을 함께 넘겨줍니다.
                target.TakeDamage(actualDamage, transform);

                // 투사체 적중 피드백
                if (FeedbackManager.Instance != null)
                {
                    if (hitStopDuration > 0f) FeedbackManager.Instance.TriggerHitStop(hitStopDuration);
                    if (shakeIntensity > 0f) FeedbackManager.Instance.TriggerCameraShake(shakeIntensity);
                }

                Deactivate();
                return;
            }
        }

        // 2. 벽이나 바닥(Ground)에 부딪혔을 때
        if (collision.gameObject.layer == GroundLayerId)
        {
            hasHit = true;
            Deactivate(); // Destroy 대신 비활성화!
        }
    }

    // 파괴하지 않고 꺼버리기만 하는 헬퍼 함수
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}