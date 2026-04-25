using UnityEngine;

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance { get; private set; }

    private void Awake()
    {
        // 씬을 이동했을 때 똑같은 [CoreManagers] 프리팹이 또 있다면,
        // 기존 것을 유지하고 새로 들어온 복제본을 통째로 파괴합니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 이 최상위 오브젝트와 그 아래에 있는 모든 자식들을 영구 보존합니다!
        DontDestroyOnLoad(gameObject);
    }
}
