using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ItemObjectPool : MonoBehaviour
{
    // 싱글톤 생성
    public static ItemObjectPool Instance { get; private set; }
    [System.Serializable]
    public class PoolInfo
    {
        public string key="Coin";    // 아이템 식별자 (예: "Coin", "Gem")
        public GameObject prefab;    // 실제 프리팹
        public int initSize = 20;    // 초기 생성 개수
        public int maxSize = 100;    // 최대 보관 개수
    }
    public List<PoolInfo> poolConfigs; 
    private Dictionary<string, ObjectPool<GameObject>> poolDict = new Dictionary<string, ObjectPool<GameObject>>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitializePools();
    }
    private void InitializePools()
    {
        foreach (var config in poolConfigs)
        {
            // 각 아이템마다 독립적인 풀을 생성
            var pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(config.prefab, transform),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: config.initSize,
                maxSize: config.maxSize
            );

            poolDict.Add(config.key, pool);

            // 초기 개수만큼 미리 생성해서 풀에 넣어두기 (선택 사항)
            List<GameObject> temp = new List<GameObject>();
            for (int i = 0; i < config.initSize; i++) temp.Add(pool.Get());
            foreach (var obj in temp) pool.Release(obj);
        }
    }
    // 아이템 꺼내기
    public GameObject GetItem(string key)
    {
        if (poolDict.ContainsKey(key))
        {
            return poolDict[key].Get();
        }
        Debug.LogWarning($"Pool with key {key} not found!");
        return null;
    }

    // 아이템 반납하기
    public void ReleaseItem(string key, GameObject obj)
    {
        if (poolDict.ContainsKey(key))
        {
            poolDict[key].Release(obj);
        }
        else
        {
            Destroy(obj); // 풀이 없으면 그냥 파괴 (예외 처리)
        }
    }
}
