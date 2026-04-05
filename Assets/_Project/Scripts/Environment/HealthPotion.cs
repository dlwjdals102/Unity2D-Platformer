using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Potion Settings")]
    public float healAmount = 20f; // 회복량

    [Header("Floating Animation")]
    public float floatSpeed = 2f;  // 둥둥거리는 속도
    public float floatHeight = 0.2f; // 둥둥거리는 높이
    private Vector3 startPos;

    private void Start()
    {
        // 시작 위치를 기억해 둡니다.
        startPos = transform.position;
    }

    private void Update()
    {
        // 수학의 사인(Sin) 함수를 이용해 위아래로 부드럽게 움직이게 만듭니다.
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    // 누군가 포션의 투명한 콜라이더(Trigger) 영역에 들어왔을 때 실행됩니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 부딪힌 오브젝트가 플레이어인지 확인합니다.
        PlayerController player = collision.GetComponent<PlayerController>();

        // 플레이어가 맞고(AND) 체력이 꽉 차지 않은 상태라면?
        if (player != null && player.CurrentHealth < player.MaxHealth)
        {
            // 1. 플레이어 회복!
            player.Heal(healAmount);

            // TODO: 나중에 여기에 '반짝!' 하는 파티클 이펙트나 '띠링~' 하는 사운드를 넣기 좋습니다.

            // 2. 포션은 사용되었으므로 맵에서 파괴(삭제)합니다.
            Destroy(gameObject);
        }
    }
}