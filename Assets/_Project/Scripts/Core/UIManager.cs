using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float defaultFadeDuration = 1f;
    [SerializeField] private Image fadeImage;

    [Header("Death UI Settings")]
    [SerializeField] private GameObject deathMenuPanel;

    [Header("Pause & Settings UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    public bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // InputManager의 Controls에 접근하여 Pause 액션이 수행될 때 TogglePause 호출
        if (InputManager.Instance != null && InputManager.Instance.Controls != null)
        {
            InputManager.Instance.Controls.UI.Pause.performed += ctx => TogglePause();
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null && InputManager.Instance.Controls != null)
        {
            InputManager.Instance.Controls.UI.Pause.performed -= ctx => TogglePause();
        }
    }

    private void Start()
    {
        // 기존 UI 끄기
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }

        if (deathMenuPanel != null)
            deathMenuPanel.SetActive(false);

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);

        // 슬라이더 초기값 세팅 및 이벤트 연결
        InitializeSettings();
    }


    // ==========================================
    // 설정창
    // ==========================================
    private void InitializeSettings()
    {
        if (bgmSlider != null)
        {
            bgmSlider.value = PlayerPrefs.GetFloat("Saved_BGM_Volume", 1f);
            bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("Saved_SFX_Volume", 1f);
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        }
    }

    public void TogglePause()
    {
        // 플레이어가 없거나(타이틀 씬), 죽었을 때는 일시정지를 막습니다.
        if (GameManager.Instance.player == null || GameManager.Instance.player.Health.IsDead) return;

        IsPaused = !IsPaused;

        if (IsPaused)
        {
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 일시정지 중에는 플레이어의 모든 조작을 강제로 차단합니다.
            // (UI 액션 맵은 켜져 있으므로 메뉴 조작은 정상 작동합니다)
            if (InputManager.Instance != null)
            {
                InputManager.Instance.Controls.Player.Disable();
            }
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenuPanel.SetActive(false);
            settingsMenuPanel.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // 일시정지가 풀리면 플레이어 조작을 다시 복구합니다.
            if (InputManager.Instance != null)
            {
                InputManager.Instance.Controls.Player.Enable();
            }
        }
    }

    // 설정창 열기/닫기 버튼용
    public void OpenSettings() { settingsMenuPanel.SetActive(true); pauseMenuPanel.SetActive(false); }
    public void CloseSettings() 
    {
        settingsMenuPanel.SetActive(false);

        // 현재 씬이 타이틀이면 아무것도 안 하고, 인게임이면 일시정지 창을 다시 엽니다.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Title")
        {
            // 타이틀 씬에서는 설정창만 닫히면 끝 (타이틀 UI는 뒤에 항상 있음)
            IsPaused = false;
        }
        else
        {
            pauseMenuPanel.SetActive(true);
        }
    }

    // [타이틀로] 버튼이 호출할 함수
    public void GoToTitle()
    {
        // 1. 시간 정상화 및 입력 복구
        Time.timeScale = 1f;
        if (InputManager.Instance != null) InputManager.Instance.Controls.Player.Enable();

        IsPaused = false;

        // 2. UI 정리
        pauseMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(false);

        // 3. 타이틀 씬으로 이동 (SceneTransitionManager 활용)
        // targetPortalID를 빈 값으로 보내면 GameManager가 텔레포트를 시도하지 않습니다.
        SceneTransitionManager.Instance.TransitionToScene("Title", "");
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
    public void ShowDeathMenu(bool show)
    {
        if (deathMenuPanel != null)
        {
            deathMenuPanel.SetActive(show);

            // 메뉴가 떴을 때 마우스 커서를 활성화하여 버튼을 누를 수 있게 합니다.
            Cursor.visible = show;
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    // 버튼 클릭 이벤트에 연결할 함수들
    public void OnClickContinue()
    {
        GameManager.Instance.ResumeRespawn();
        ShowDeathMenu(false);
    }

    public void OnClickQuit()
    {
        // SceneTransitionManager를 통해 타이틀 씬으로 이동 (타이틀 씬 이름이 "Title"이라고 가정)
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene("Title", "None");
        }
    }
}
