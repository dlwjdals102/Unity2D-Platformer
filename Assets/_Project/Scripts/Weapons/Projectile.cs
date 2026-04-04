using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // 이 스크립트를 넣으면 Rigidbody2D가 자동으로 붙습니다!
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;       // 날아가는 속도
    public float lifeTime = 3f;     // 3초 뒤 자동 소멸 (화면 밖으로 날아간 투사체 정리)

    private Rigidbody2D rb;
    private float actualDamage; // 인스펙터에서 빼고, 발사자가 넘겨준 데미지를 기억할 변수

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // 맵 끝까지 날아가서 영원히 메모리를 차지하는 것을 방지하기 위한 타이머입니다.
        Destroy(gameObject, lifeTime);
    }

    // 몬스터가 이 투사체를 소환(Instantiate)한 직후에 호출할 함수입니다.
    public void Setup(Vector2 moveDirection, float damageAmount)
    {
        actualDamage = damageAmount;

        // 방향을 정규화(길이를 1로 만듦)한 뒤 속도를 곱해 날려 보냅니다.
        rb.linearVelocity = moveDirection.normalized * speed;

        // 투사체 이미지(화살촉 등)가 날아가는 방향을 바라보도록 회전시킵니다.
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 투사체가 무언가에 부딪혔을 때의 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 플레이어에게 맞았을 때
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(actualDamage);
            Destroy(gameObject); // 데미지를 입히고 파괴
            return; // 아래 코드를 실행하지 않고 바로 종료
        }

        // 2. 벽이나 바닥(Ground)에 부딪혔을 때
        // (프로젝트의 바닥 레이어 이름이 "Ground"라고 가정합니다. 다를 경우 수정해 주세요!)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // TODO: 나중에 '벽에 화살이 박히는 이펙트'나 '흙먼지 파티클'을 넣기 좋은 위치입니다.
            Destroy(gameObject);
        }
    }
}