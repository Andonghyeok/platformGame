using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    // 싱글톤
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolInfo
    {
        public string key;           // 식별자 (예: "Coin", "M_RedSlime")
        public GameObject prefab;    // 인스펙터에서 할당할 프리팹 (어드레서블은 비워둬도 됨)
        public int initSize = 20;
        public int maxSize = 100;
    }

    public List<PoolInfo> poolConfigs;
    private Dictionary<string, ObjectPool<GameObject>> poolDict = new Dictionary<string, ObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 만약 씬이 넘어가도 유지하고 싶다면 이 줄을 추가하세요.
        }
        else
        {
            Debug.LogWarning("중복된 풀 매니저가 발견되어 파괴합니다.");
            Destroy(gameObject); // <- 이 줄 때문에 사라지는 것입니다!
            return;
        }
        InitializePools();
    }

    private void InitializePools()
    {
        // 인스펙터에 미리 등록해둔 아이템/기본 몬스터들을 풀에 생성
        foreach (var config in poolConfigs)
        {
            CreatePool(config.key, config.prefab, config.initSize, config.maxSize);
        }
    }

    // [추가] 런타임에 동적으로 풀을 만드는 기능 (어드레서블 몬스터 연동용)
    public void CreatePool(string key, GameObject prefab, int initSize = 10, int maxSize = 100)
    {
        if (poolDict.ContainsKey(key)) return; // 이미 풀이 있다면 패스

        var pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: initSize,
            maxSize: maxSize
        );

        poolDict.Add(key, pool);

        // 미리 생성 (prefab이 null이 아닐 때만)
        if (prefab != null && initSize > 0)
        {
            List<GameObject> temp = new List<GameObject>();
            for (int i = 0; i < initSize; i++) temp.Add(pool.Get());
            foreach (var obj in temp) pool.Release(obj);
        }
    }

    // 오브젝트 꺼내기 (이름 변경: GetItem -> GetObject)
    public GameObject GetObject(string key)
    {
        if (poolDict.ContainsKey(key))
        {
            return poolDict[key].Get();
        }

        // 풀이 없으면 null 반환 (Spawner에서 이 값을 보고 Addressable로 생성하도록 유도)
        return null;
    }

    // 오브젝트 반납하기 (이름 변경: ReleaseItem -> ReleaseObject)
    public void ReleaseObject(string key, GameObject obj)
    {
        if (poolDict.ContainsKey(key))
        {
            poolDict[key].Release(obj);
        }
        else
        {
            // [중요] 풀이 없는데 반납이 들어왔다? 
            // 1. 어드레서블로 소환된 객체일 수 있으므로 풀을 새로 만들어줍니다.
            // 2. 프리팹 원본은 모르지만, 일단 빈 풀을 만들어서 보관합니다.
            var newPool = new ObjectPool<GameObject>(
                createFunc: () => null, // 어드레서블 객체는 여기서 Instantiate 하지 않음
                actionOnGet: (o) => o.SetActive(true),
                actionOnRelease: (o) => o.SetActive(false),
                actionOnDestroy: (o) => Destroy(o)
            );
            poolDict.Add(key, newPool);
            newPool.Release(obj);
        }
    }
}