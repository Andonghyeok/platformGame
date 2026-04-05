using UnityEngine;

public class EffectHandler : MonoBehaviour
{
    PlayerStatModifier stats;

    void Awake()
    {
        stats = GetComponent<PlayerStatModifier>();
    }

    public void ApplyEffect(int itemID)
    {
        // 1. 인스턴스 확인부터!
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("!! ItemDatabase 인스턴스가 존재하지 않습니다 !!");
            return;
        }

        // 2. 데이터 가져오기
        ItemRow data = ItemDatabase.Instance.GetItem(itemID);

        // 3. 데이터가 없을 때의 처리 (여기서 로그를 찍어야 왜 없는지 알 수 있습니다)
        if (data == null)
        {
            Debug.LogError($"[ApplyEffect] ID {itemID}에 해당하는 데이터를 찾을 수 없습니다. DB를 확인하세요!");
            return;
        }

        // 4. 이제서야 성공 로그 출력
        Debug.Log($"1. ApplyEffect 진입 성공! ID: {itemID}, 이름: {data.name}");


        Debug.Log($"{data.name} 획득: {data.description}");
        // 이런식 말고 팀 회복이나 그냥 솔로 회복일 수도 있음 그리고 아이템 갯수가 100개 이상일때는 힘듬 하드 코딩임
        if (itemID == 101)   
        {
            stats.CurrentHealth = Mathf.Min(stats.CurrentHealth + data.effectAmount, stats.MaxHealth.Value);
        }
        else if (itemID == 102) 
        {
            var speedMod = new StatModifier(data.effectAmount, StatModType.Flat, this);

            stats.AddTimedModifier(stats.MoveSpeed, speedMod, 5f);

            StartCoroutine(FlashColor(Color.cyan, 5f));
        }
        else if (itemID == 201)  // Poison (일정 시간 이동속도 감소)
        {
            var poisonMod = new StatModifier(data.effectAmount, StatModType.Flat, this);
            stats.AddTimedModifier(stats.MoveSpeed, poisonMod, 2f); // 2초 지속

            StartCoroutine(FlashColor(Color.green, 2f));
        }

    }

    // 버프/디버프 동안 플레이어 색깔 변경용 코루틴
    System.Collections.IEnumerator FlashColor(Color color, float duration)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = color;
            yield return new WaitForSeconds(duration);
            sr.color = original;
        }
    }
}