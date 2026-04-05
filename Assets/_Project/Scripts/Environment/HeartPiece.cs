using UnityEngine;

public class HeartPiece : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public float healthIncreaseAmount = 20f; // 최대 체력이 얼마나 늘어날지

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            // 플레이어의 최대 체력을 영구적으로 증가시킴
            player.IncreaseMaxHealth(healthIncreaseAmount);

            // TODO: 나중에 여기에 화려한 파티클이나 사운드를 추가합니다.
            Destroy(gameObject);
        }
    }
}
