using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("UI Settings")]
    public Image fadeImage;          // 화면을 덮을 검은색 이미지
    public float fadeDuration = 1f;  // 페이드 아웃/인에 걸리는 시간

    // ==========================================
    // 외부에서 언제든 호출할 수 있는 단일 페이드 함수들
    // ==========================================
    public void PlayFadeOut() => StartCoroutine(FadeOutCoroutine());
    public void PlayFadeIn() => StartCoroutine(FadeInCoroutine());

    // ==========================================
    // 모듈화된 핵심 코루틴 (재사용 가능)
    // ==========================================
    private IEnumerator FadeOutCoroutine()
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
    }

    private IEnumerator FadeInCoroutine()
    {
        if (fadeImage == null) yield break;

        Color c = fadeImage.color;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
    }

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

    private void Start()
    {
        // 씬이 처음 시작될 때 화면이 까만색에서 서서히 밝아지도록(Fade-In) 만듭니다.
        StartCoroutine(FadeInCoroutine());
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
        // 1. 페이드 아웃 모듈 실행 후 완료될 때까지 대기
        yield return StartCoroutine(FadeOutCoroutine());

        // 2. 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 3. 새 씬의 명찰 찾기 및 깜빡임 방지 세팅
        FadeUI newFadeUI = FindFirstObjectByType<FadeUI>();
        if (newFadeUI != null)
        {
            fadeImage = newFadeUI.fadeImage;
            fadeImage.gameObject.SetActive(true);
            Color startColor = fadeImage.color;
            startColor.a = 1f;
            fadeImage.color = startColor;
        }

        // 4. 페이드 인 모듈 실행
        yield return StartCoroutine(FadeInCoroutine());
    }
}
