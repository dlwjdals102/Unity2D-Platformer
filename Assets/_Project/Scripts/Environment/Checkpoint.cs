using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 켜진 체크포인트면 무시, 부딪힌 게 플레이어가 아니어도 무시
        if (isActivated) return;

        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            isActivated = true;

            // GameManager에게 이 체크포인트의 위치를 전달합니다!
            GameManager.Instance.UpdateRespawnPoint(transform.position);

            // TODO: 나중에 여기에 '깃발 펄럭임' 애니메이션이나 파티클을 넣습니다.
        }
    }
}
