using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [System.Serializable]
    public class GameData
    {
        public float currentHealth;
        public float maxHealth;
        //public int gold;
        public string lastPortalID; // 어떤 포탈로 나왔는지 기억
        // 여기에 언제든 public 변수를 추가하면 즉시 동기화 대상으로 포함됩니다.
    }

    public GameData sessionData = new GameData();

    public bool hasSavedData = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
