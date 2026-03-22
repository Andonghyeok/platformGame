using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MonsterSpawner : MonoBehaviour
{
    public SpawnerTable spawnerTable;
    public MonsterTable monsterTable;

    void Start()
    {
        if (spawnerTable == null || monsterTable == null) return;

        foreach (var spawnData in spawnerTable.spawnList)
        {
            // 각 슬롯별로 최초 소환 시작
            StartCoroutine(SpawnMonsterRoutine(spawnData));
        }
    }

    IEnumerator SpawnMonsterRoutine(SpawnerData data)
    {
        // 1. 초기 딜레이
        yield return new WaitForSeconds(data.spawnStartTime);

        // 2. 실제 소환 실행
        Spawn(data);
    }

    void Spawn(SpawnerData data)
    {
        MonsterData stat = monsterTable.monsterList.Find(x => x.monsterId == data.monsterId);
        if (stat == null) return;

        string cleanKey = stat.prefabKey.Trim();
        float randomX = Random.Range(-data.range, data.range);
        Vector3 spawnPos = new Vector3(transform.position.x + randomX, transform.position.y, 0);

        GameObject pooledMonster = ObjectPoolManager.Instance.GetObject(cleanKey);

        if (pooledMonster != null)
        {
            pooledMonster.transform.position = spawnPos;
            pooledMonster.transform.rotation = Quaternion.identity;

            // [중요] 몬스터에게 자신이 태어난 스패너 정보를 넘겨줌 (나중에 죽을 때 알려주기 위해)
            InitMonster(pooledMonster, stat, data);
        }
        else
        {
            Addressables.InstantiateAsync(cleanKey, spawnPos, Quaternion.identity).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    InitMonster(handle.Result, stat, data);
                }
            };
        }
    }

    void InitMonster(GameObject monsterGo, MonsterData data, SpawnerData sData)
    {
        var enemy = monsterGo.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 몬스터에게 스패너 정보와 현재 스폰 데이터를 전달
            enemy.Setup(data, sData, this);
        }

        // 나머지 공격력 등 셋팅...
    }

    // [핵심] 몬스터가 죽었을 때 호출될 함수
    public void OnMonsterDead(SpawnerData data)
    {
        // 일정 시간(예: 3초) 뒤에 다시 Spawn 함수를 실행
        StartCoroutine(ReSpawnDelayRoutine(data));
    }

    IEnumerator ReSpawnDelayRoutine(SpawnerData data)
    {
        yield return new WaitForSeconds(3.0f); // 리스폰 간격 (원하는 초로 수정 가능)
        Spawn(data);
    }
}