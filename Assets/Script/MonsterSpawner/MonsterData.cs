using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MonsterData
{
    public int monsterId;      // 몬스터 ID 
    public string name;        // 몬스터 이름
    public string prefabKey;   // 프리팹 이름 
    public int maxHp;          // 최대 체력
    public int atk;            // 공격력
    public int def;            // 방어력
    public float moveSpeed;    // 이동 속도
    public int expReward;      // 경험치 보상
}

[CreateAssetMenu(fileName = "MonsterTable", menuName = "ScriptableObjects/MonsterTable")]
public class MonsterTable : ScriptableObject
{
    public List<MonsterData> monsterList = new List<MonsterData>();
}