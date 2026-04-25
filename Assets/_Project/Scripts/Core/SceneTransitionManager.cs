using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private bool isTransitioning = false; // 중복 실행 방지 플래그

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(transform.root.gameObject); // 씬이 넘어가도 매니저 파괴 방지
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 포탈이 이 함수를 호출하여 씬 이동을 요청합니다.
    public void TransitionToScene(string sceneName, string targetPortalID)
    {
        if (isTransitioning) return;

        // (씬 이동을 시작하기 직전 데이터 저장 파트)
        if (GameManager.Instance != null && DataManager.Instance != null)
        {
            // 플레이어가 존재하고, 목적지가 타이틀이 아닐 때만 "진짜 세이브" 실행
            if (GameManager.Instance.player != null && sceneName != "Title")
            {
                DataManager.GameData package = GameManager.Instance.ExportPlayerSession();

                // 다음에 이어하기를 눌렀을 때 '도착할 씬'과 '포탈'을 저장합니다.
                package.lastPortalID = targetPortalID;
                package.lastSceneName = sceneName;

                DataManager.Instance.SaveTransitionData(package);
                Debug.Log("[SceneTransition] 플레이어 데이터를 성공적으로 포장했습니다.");
            }
        }

        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        isTransitioning = true;

        // UI 연출은 UIManager에게 위임합니다.
        if (UIManager.Instance != null)
            yield return StartCoroutine(UIManager.Instance.FadeOut());

        // 씬 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null; // 로드할때까지 대기

        // 핵심: 씬이 로드된 후 '검은 화면 상태'에서 0.5초 정도 강제로 대기합니다.
        // 이 시간 동안 시네머신 카메라가 플레이어의 위치로 몰래 날아가서 자리를 잡습니다.
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(UIManager.Instance.FadeIn(1.5f));

        isTransitioning = false;
    }
}
