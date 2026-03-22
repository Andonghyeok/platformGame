using UnityEngine;

public class MonsterController : MonoBehaviour
{
    // 엑셀에서 가져온 능력치를 저장할 변수
    public MonsterData myStat;

    // 이 함수가 있어야 Spawner에서 에러 없이 데이터를 넘겨줄 수 있습니다!
    public void SetStat(MonsterData data)
    {
        myStat = data;

        // 잘 들어왔는지 확인용 로그
        Debug.Log($"{myStat.name} 능력치 설정 완료! HP: {myStat.maxHp}, ATK: {myStat.atk}");

        // 여기에 체력바를 갱신하거나 몬스터의 속도를 설정하는 로직을 넣으면 됩니다.
    }
}