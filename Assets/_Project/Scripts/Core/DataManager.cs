using UnityEngine;

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
        public float currentMana; // 세션 마나 데이터
        public float maxMana; // 세션 최대 마나 데이터

        public string lastSceneName; // 마지막으로 머물렀던 씬 이름
        public string lastPortalID; // 어떤 포탈로 나왔는지 기억

        // 여기에 언제든 public 변수를 추가하면 즉시 동기화 대상으로 포함됩니다.
        // 추가: 보스 처치 여부를 저장하는 플래그
        public bool isBossDefeated;
    }

    [Header("Default Data Assets")]
    [SerializeField] private PlayerData defaultPlayerData; // 에디터에서 PlayerData SO를 연결합니다.


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

    public void ClearData()
    {
        sessionData = new GameData();

        if (defaultPlayerData != null)
        {
            // SO 설계도에서 초기 스탯을 복사해옵니다.
            sessionData.maxHealth = defaultPlayerData.maxHealth;
            sessionData.currentHealth = defaultPlayerData.maxHealth;
            sessionData.maxMana = defaultPlayerData.maxMana;
            sessionData.currentMana = defaultPlayerData.maxMana;
        }

        hasSavedData = false;
        Debug.Log("[DataManager] 새 게임을 위해 세션 데이터가 SO 기반으로 초기화되었습니다.");
    }

    /// <summary>
    /// 포장된 GameData 상자를 통째로 넘겨받아 전역 창고에 안전하게 저장합니다.
    /// </summary>
    public void SaveTransitionData(GameData newData) // 매개변수가 하나로 압축됨!
    {
        // 1. 데이터 검증 (결함이 있는 데이터가 들어오는 것을 방지)
        if (newData.currentHealth < 0) newData.currentHealth = 0;

        // 2. 창고(sessionData)에 덮어쓰기
        sessionData = newData;
        hasSavedData = true;

        Debug.Log($"[Save] 씬 전환 데이터 저장 완료: 체력({sessionData.currentHealth}/{sessionData.maxHealth}), 목적지({sessionData.lastPortalID})");

        // 3. (확장성) 추후 SaveToJsonFile() 한 줄만 추가하면 완벽한 세이브 시스템이 됩니다.
    }
}
