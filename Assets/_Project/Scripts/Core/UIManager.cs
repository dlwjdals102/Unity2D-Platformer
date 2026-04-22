using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float defaultFadeDuration = 1f;
    private Image fadeImage;

    [Header("Death UI Settings")]
    [SerializeField] private CanvasGroup deathTextGroup; // "YOU DIED" 텍스트의 투명도 조절용

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // UI는 씬 전환 시에도 유지되어야 합니다.
    }

    private void Start()
    {
        FindFadeImage();
        if (deathTextGroup != null) deathTextGroup.alpha = 0f;
    }

    // 씬이 바뀔 때마다 새로운 씬의 FadeUI를 찾아 연결합니다.
    public void FindFadeImage()
    {
        FadeUI foundFade = FindFirstObjectByType<FadeUI>();
        if (foundFade != null)
        {
            fadeImage = foundFade.fadeImage;
            // 시작 시 화면이 어두울 경우를 대비해 세팅
            Color c = fadeImage.color;
            c.a = (fadeImage.gameObject.activeSelf) ? c.a : 0f;
            fadeImage.color = c;
        }
    }

    // ==========================================
    // 페이드 로직
    // ==========================================
    public IEnumerator FadeOut(float duration = -1f)
    {
        float targetDuration = (duration < 0) ? defaultFadeDuration : duration;
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float timer = 0f;

        while (timer < targetDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, timer / targetDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        float targetDuration = (duration < 0) ? defaultFadeDuration : duration;
        if (fadeImage == null) yield break;

        Color c = fadeImage.color;
        float timer = 0f;

        while (timer < targetDuration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, timer / targetDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(false);
    }

    // ==========================================
    // 사망 텍스트 연출
    // ==========================================
    public IEnumerator ShowDeathText(float duration = 1f)
    {
        if (deathTextGroup == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            deathTextGroup.alpha = Mathf.Lerp(0f, 1f, timer / duration);
            yield return null;
        }
        deathTextGroup.alpha = 1f;
    }

    public IEnumerator HideDeathText(float duration = 0.5f)
    {
        if (deathTextGroup == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            deathTextGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            yield return null;
        }
        deathTextGroup.alpha = 0f;
    }
}
