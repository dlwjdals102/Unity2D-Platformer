using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // 씬이 넘어가도 매니저 파괴 방지
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
        // 1. 빈 택배 상자(GameData)를 하나 준비합니다.
        DataManager.GameData dataToSave = new DataManager.GameData();

        // 2. 현재 씬의 정보들을 상자에 담습니다.
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            dataToSave.currentHealth = GameManager.Instance.player.Health.CurrentHealth;
            dataToSave.maxHealth = GameManager.Instance.player.Health.MaxHealth;
        }

        dataToSave.lastPortalID = targetPortalID;

        // 3. 포장이 끝난 상자를 DataManager에게 전달합니다.
        DataManager.Instance.SaveTransitionData(dataToSave);

        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        // UI 연출은 UIManager에게 위임합니다.
        if (UIManager.Instance != null)
            yield return StartCoroutine(UIManager.Instance.FadeOut());

        // 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 새 씬의 UI 요소를 다시 찾도록 지시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.FindFadeImage();
            yield return StartCoroutine(UIManager.Instance.FadeIn());
        }
    }
}
