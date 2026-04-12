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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // (풀에서 꺼내져 활성화될 때마다 실행됨)
    private void OnEnable()
    {
        // lifeTime 뒤에 Deactivate 함수를 실행하도록 예약합니다.
        Invoke(nameof(Deactivate), lifeTime);
    }

    private void OnDisable()
    {
        // 비활성화될 때 예약된 타이머를 취소합니다. (버그 방지)
        CancelInvoke();
    }

    // 몬스터가 이 투사체를 소환(Instantiate)한 직후에 호출할 함수입니다.
    public void Setup(Vector2 moveDirection, float damageAmount, LayerMask targetMask)
    {
        actualDamage = damageAmount;
        targetLayer = targetMask;

        // 방향을 정규화(길이를 1로 만듦)한 뒤 속도를 곱해 날려 보냅니다.
        rb.linearVelocity = moveDirection.normalized * speed;

        // 투사체 이미지(화살촉 등)가 날아가는 방향을 바라보도록 회전시킵니다.
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 투사체가 무언가에 부딪혔을 때의 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 대상이 targetLayer에 포함되는지(적군인지) 먼저 검사합니다!
        if ((targetLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // 이제 이 투사체는 플레이어, 파괴 가능한 사물, (만약 플레이어가 쏜다면) 몬스터까지 모두 타격 가능합니다.
            IDamageable target = collision.GetComponent<IDamageable>();
            if (target != null)
            {
                // 넉백 방향 계산을 위해 transform(투사체의 위치)을 함께 넘겨줍니다.
                target.TakeDamage(actualDamage, transform);
                Deactivate();
                return;
            }
        }

        // 2. 벽이나 바닥(Ground)에 부딪혔을 때
        // (프로젝트의 바닥 레이어 이름이 "Ground"라고 가정합니다. 다를 경우 수정해 주세요!)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // TODO: 나중에 '벽에 화살이 박히는 이펙트'나 '흙먼지 파티클'을 넣기 좋은 위치입니다.
            Deactivate(); // Destroy 대신 비활성화!
        }
    }

    // 파괴하지 않고 꺼버리기만 하는 헬퍼 함수
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}