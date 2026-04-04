using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // 씬에 GameManager가 단 하나만 존재하도록 보장합니다.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 오버를 처리하는 전담 메서드
    public void GameOver()
    {
        Debug.Log("[GameManager] 게임 오버! 씬을 재시작합니다.");

        // TODO: 나중에 여기에 화면 Fade Out이나 Game Over UI 띄우는 로직을 추가하기 아주 좋습니다!

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}