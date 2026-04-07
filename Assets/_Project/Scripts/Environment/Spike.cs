using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    // 트리거 체크를 통해 닿는 순간 데미지를 줍니다.
    private void OnTriggerStay2D(Collider2D collision)
    {
        // 닿은 대상이 IDamageable(플레이어 포함)인지 확인합니다.
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            // 인터페이스를 통해 데미지 전달! 
            // 플레이어의 무적 시간(i-frame) 덕분에 매 프레임 다다닥 맞지 않고 1초에 한 번만 맞게 됩니다.
            target.TakeDamage(damage, transform);
        }
    }
}
