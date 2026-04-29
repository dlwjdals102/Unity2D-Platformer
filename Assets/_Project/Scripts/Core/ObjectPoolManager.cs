using System.Collections.Generic;
using UnityEngine;

// 에디터에서 풀(Pool)의 종류와 크기를 설정할 수 있는 클래스
[System.Serializable]
public class Pool
{
    public string tag;           // 예: "Arrow", "Fireball"
    public GameObject prefab;    // 생성할 프리팹
    public int size;             // 미리 만들어둘 개수 (예: 20개)
}

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        // 표준 싱글톤 패턴 (DontDestroyOnLoad는 부모 CoreManager가 처리)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 창고(딕셔너리) 초기화
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // 지정된 개수만큼 미리 생성해서 꺼둡니다(비활성화).
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // 창고에서 물건을 빌려주는 함수
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        // 1. 대기열에서 가장 오래 쉰 오브젝트를 꺼냅니다.
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        // 2. 위치와 회전값을 맞추고 활성화합니다.
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        // 3. 다시 대기열의 맨 뒤로 집어넣습니다. (순환 구조)
        // 이렇게 하면 반납 처리를 따로 할 필요 없이 꺼졌다 켜졌다를 반복하며 무한 재사용됩니다!
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
