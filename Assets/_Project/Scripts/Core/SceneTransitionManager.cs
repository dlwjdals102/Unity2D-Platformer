using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private Coroutine activeTransition; // 현재 진행 중인 전환 코루틴 (중복 방지 + 교체용)

    private void Awake()
    {
        // 표준 싱글톤 패턴 (DontDestroyOnLoad는 부모 CoreManager가 처리)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 다른 씬으로 이동합니다. (포탈 등)
    /// 플레이어 데이터를 자동 저장 후 페이드/로드/페이드인을 처리합니다.
    /// </summary>
    public void TransitionToScene(string sceneName, string targetPortalID)
    {
        // 기존 전환이 진행 중이면 즉시 중단하고 새 전환으로 교체
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }

        // 씬 이동 직전 플레이어 데이터 저장 (이어하기를 위함)
        SavePlayerSession(sceneName, targetPortalID);

        activeTransition = StartCoroutine(FadeOutAndLoad(sceneName));
    }

    /// <summary>
    /// 현재 씬을 다시 로드합니다. (사망 부활 등)
    /// 데이터 저장은 호출자가 미리 처리한 상태여야 합니다 (체력 풀 회복 등 특수 처리 필요).
    /// </summary>
    public void ReloadCurrentScene()
    {
        // 기존 전환이 진행 중이면 즉시 중단하고 새 전환으로 교체
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }

        string currentScene = SceneManager.GetActiveScene().name;
        activeTransition = StartCoroutine(FadeOutAndLoad(currentScene));
    }

    /// <summary>
    /// 플레이어 세션 데이터를 다음 씬 정보와 함께 저장합니다.
    /// (TransitionToScene에서만 사용 — ReloadCurrentScene은 호출자가 미리 저장)
    /// </summary>
    private void SavePlayerSession(string sceneName, string targetPortalID)
    {
        if (GameManager.Instance == null || DataManager.Instance == null) return;

        // 플레이어가 존재하고, 목적지가 타이틀이 아닐 때만 데이터 저장
        if (GameManager.Instance.player != null && sceneName != Define.SceneNames.Title)
        {
            DataManager.GameData package = GameManager.Instance.ExportPlayerSession();

            // 이어하기를 위한 도착 위치 저장
            package.lastPortalID = targetPortalID;
            package.lastSceneName = sceneName;

            DataManager.Instance.SaveTransitionData(package);
        }
    }

    /// <summary>
    /// 씬 전환의 시각 효과 + 로딩 시퀀스.
    /// FadeOut > 로드 > 카메라 자리잡기 대기 > FadeIn 까지 한 번에 처리합니다.
    /// </summary>
    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        // 1. 화면 어둡게
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartFadeOut();
            yield return new WaitUntil(() => UIManager.Instance.IsFadeComplete);
        }

        // 2. 씬 비동기 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // 3. 어둠 속에서 0.5초 대기 — 시네머신 카메라가 새 플레이어 위치로 자리 잡는 시간
        yield return new WaitForSeconds(0.5f);

        // 4. 화면 다시 밝게
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartFadeIn(1.5f);
            yield return new WaitUntil(() => UIManager.Instance.IsFadeComplete);
        }

        activeTransition = null;
    }

}
