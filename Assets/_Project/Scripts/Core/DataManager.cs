using System.IO;
using UnityEngine;


/// <summary>
/// 게임 전반의 영속 데이터를 관리하는 매니저입니다.
///
/// [데이터 흐름]
/// - sessionData: 현재 세션의 모든 진행 상황 (체력, 마나, 위치, 보스 처치 여부 등)
/// - 메모리 → 파일: SaveToFile()로 JSON 직렬화하여 디스크에 영구 보존
/// - 파일 → 메모리: TryLoadFromFile()로 디스크에서 복구
/// - 씬 전환: SaveTransitionData()로 메모리에만 임시 저장 (성능 + 의도적 명시 저장 분리)
///
/// 저장 위치는 Application.persistentDataPath를 사용하여 플랫폼 독립적으로 동작합니다.
/// (Windows: %userprofile%\AppData\LocalLow\..., Mac: ~/Library/Application Support/...)
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public GameData sessionData;
    public bool hasSavedData = false;

    [System.Serializable]
    public class GameData
    {
        public float currentHealth;
        public float maxHealth;
        public float currentMana;
        public float maxMana;

        public string lastSceneName; // 마지막으로 머물렀던 씬 이름

        // 위치 복원에 사용되는 ID들 (둘 중 하나만 사용)
        // 우선순위: 체크포인트 > 포탈 (체크포인트 저장이 더 최근일 때)
        public string lastPortalID;     // 포탈로 씬 이동했을 때 도착할 포탈 ID
        public string lastCheckpointID; // 체크포인트에서 저장했을 때 부활할 체크포인트 ID

        public bool isBossDefeated;  // 보스 처치 여부

        // 새 필드를 여기 추가하면 자동으로 JSON 직렬화에 포함됩니다.
    }

    [Header("Default Data Assets")]
    [SerializeField] private PlayerData defaultPlayerData; // 에디터에서 PlayerData SO를 연결합니다.

    [Header("Save File Settings")]
    [Tooltip("저장 파일의 이름 (확장자 포함)")]
    [SerializeField] private string saveFileName = "save.json";

    /// <summary>플랫폼 독립적 저장 경로 (Windows/Mac/Linux/Mobile 모두 자동으로 적절한 위치 사용)</summary>
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

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

    private void Start()
    {
        // 게임 부팅 시 저장 파일이 있는지 자동 점검 (타이틀의 [이어하기] 버튼 활성화 판단용)
        bool loaded = TryLoadFromFile();

        // 로드 실패 시(첫 실행 또는 파일 손상) 빈 sessionData를 생성합니다.
        // 이렇게 하면 어디서든 sessionData에 접근할 때 NRE가 발생하지 않도록 보장합니다.
        // (예: 새 게임을 거치지 않고 테스트 씬으로 직행하는 경우 등)
        if (!loaded)
        {
            sessionData = new GameData();
        }
    }

    // ==========================================
    // 1. 세션 데이터 조작 (메모리)
    // ==========================================

    /// <summary>
    /// 새 게임을 시작할 때 호출됩니다. 세션 데이터를 SO 기본값으로 초기화합니다.
    /// </summary>
    public void ClearData()
    {
        sessionData = new GameData();

        if (defaultPlayerData != null)
        {
            sessionData.maxHealth = defaultPlayerData.maxHealth;
            sessionData.currentHealth = defaultPlayerData.maxHealth;
            sessionData.maxMana = defaultPlayerData.maxMana;
            sessionData.currentMana = defaultPlayerData.maxMana;
        }

        hasSavedData = false;

        // 저장 파일도 함께 삭제 (새 게임이므로)
        DeleteSaveFile();
    }

    /// <summary>
    /// 씬 전환 시점에 호출됩니다. 메모리에만 저장(빠름)하고, 디스크에도 자동으로 저장합니다.
    ///
    /// [중요] 새 데이터(newData)에는 플레이어 스탯과 위치 정보만 들어 있습니다.
    /// 보스 처치 여부 같은 "영구 진행 정보"는 PlayerController가 알 필요가 없으므로
    /// 여기서 기존 sessionData의 값을 보존(merge)합니다.
    /// 이렇게 하면 관심사가 명확하게 분리됩니다:
    ///   - PlayerController: 자기 스탯(체력/마나)만 export
    ///   - DataManager: 영구 진행 정보를 책임지고 보존
    /// </summary>
    public void SaveTransitionData(GameData newData)
    {
        // 1. 데이터 검증
        if (newData.currentHealth < 0) newData.currentHealth = 0;
        if (newData.currentMana < 0) newData.currentMana = 0;

        // 2. 영구 진행 정보 병합 (기존 sessionData에서 가져와 newData에 주입)
        //    새 영구 진행 필드를 추가할 때마다 여기에 한 줄씩 추가하면 됩니다.
        if (sessionData != null)
        {
            newData.isBossDefeated = sessionData.isBossDefeated;
            // 예시: newData.unlockedSkills = sessionData.unlockedSkills;
            // 예시: newData.collectedItems = sessionData.collectedItems;
        }

        // 3. 메모리 갱신
        sessionData = newData;
        hasSavedData = true;

        // 4. 디스크 저장 (씬 전환마다 자동 저장 = 체크포인트 시스템)
        SaveToFile();

        Debug.Log($"[Save] 씬 전환 데이터 저장 완료: 체력({sessionData.currentHealth}/{sessionData.maxHealth}), 목적지({sessionData.lastPortalID})");
    }

    // ==========================================
    // 2. JSON 영속 저장 (디스크)
    // ==========================================

    /// <summary>
    /// 현재 sessionData를 JSON 파일로 디스크에 저장합니다.
    /// </summary>
    public void SaveToFile()
    {
        if (sessionData == null)
        {
            Debug.LogWarning("[DataManager] 저장할 sessionData가 null입니다.");
            return;
        }

        try
        {
            // JsonUtility.ToJson의 두 번째 인자 true = 사람이 읽기 좋게 들여쓰기
            string json = JsonUtility.ToJson(sessionData, true);
            File.WriteAllText(SavePath, json);

            Debug.Log($"[DataManager] 세이브 파일 저장 완료: {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataManager] 세이브 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 디스크의 JSON 파일에서 sessionData를 복원합니다.
    /// </summary>
    /// <returns>로드 성공 시 true. 파일이 없거나 손상된 경우 false.</returns>
    public bool TryLoadFromFile()
    {
        // 1. 파일 존재 확인
        if (!File.Exists(SavePath))
        {
            hasSavedData = false;
            return false;
        }

        try
        {
            // 2. JSON 읽고 역직렬화
            string json = File.ReadAllText(SavePath);
            GameData loaded = JsonUtility.FromJson<GameData>(json);

            // 3. 손상된 데이터 검증 (역직렬화 결과가 null이거나 maxHealth가 0인 경우)
            if (loaded == null || loaded.maxHealth <= 0)
            {
                Debug.LogWarning("[DataManager] 세이브 파일이 손상되었거나 비어 있습니다. 무시합니다.");
                hasSavedData = false;
                return false;
            }

            sessionData = loaded;
            hasSavedData = true;

            Debug.Log($"[DataManager] 세이브 파일 로드 완료: 씬={sessionData.lastSceneName}, 체력={sessionData.currentHealth}/{sessionData.maxHealth}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataManager] 세이브 로드 실패: {e.Message}");
            hasSavedData = false;
            return false;
        }
    }

    /// <summary>
    /// 디스크의 저장 파일을 삭제합니다 (새 게임 시작 시 호출).
    /// </summary>
    public void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                File.Delete(SavePath);
                Debug.Log("[DataManager] 세이브 파일 삭제 완료.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataManager] 세이브 파일 삭제 실패: {e.Message}");
            }
        }
    }
}
