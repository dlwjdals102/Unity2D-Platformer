using UnityEngine;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    private void Start()
    {
        // 1. 저장된 데이터가 있는지 확인하여 이어하기 버튼 활성/비활성
        if (continueButton != null)
        {
            // DataManager가 없으면 이어하기 비활성화
            continueButton.interactable = (DataManager.Instance != null) && DataManager.Instance.hasSavedData;
        }

        // 마우스 커서 활성화 (타이틀 씬 필수)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AudioManager.Instance?.PlayBGM("BGM_Title");
    }

    public void OnClickNewGame()
    {
        // 새 게임 시작 시 기존 데이터 완전 삭제
        if (DataManager.Instance != null && SceneTransitionManager.Instance != null)
        {
            DataManager.Instance.ClearData();

            // 새 게임의 시작 위치를 sessionData에 직접 명시
            DataManager.Instance.sessionData.lastSceneName = Define.SceneNames.Stage_1;
            DataManager.Instance.sessionData.lastPortalID = "StartPortal";
            DataManager.Instance.hasSavedData = true; // 위치 복원 로직 활성화

            SceneTransitionManager.Instance.TransitionToScene(
                DataManager.Instance.sessionData.lastSceneName,
                DataManager.Instance.sessionData.lastPortalID);
        }

    }

    public void OnClickContinue()
    {
        if (DataManager.Instance == null || !DataManager.Instance.hasSavedData) return;
        if (SceneTransitionManager.Instance == null) return;

        string savedScene = DataManager.Instance.sessionData.lastSceneName;
        string savedPortal = DataManager.Instance.sessionData.lastPortalID;

        // 만약 데이터가 비어있다면 기본값 설정 (방어 코드)
        if (string.IsNullOrEmpty(savedScene)) savedScene = Define.SceneNames.Stage_1;

        // 실제 프로젝트 구조에 따라 씬 이름을 데이터에서 가져오도록 확장 가능합니다.
        SceneTransitionManager.Instance.TransitionToScene(savedScene, savedPortal);
    }

    public void OnClickSettings()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenSettings();
        }
    }

    /// <summary>
    /// [Quit] 버튼을 눌렀을 때 실행됩니다.
    /// </summary>
    public void QuitGame()
    {

        // 에디터에서는 플레이 모드를 정지하고, 실제 빌드된 게임에서는 창을 닫습니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
