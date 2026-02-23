using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public string itemName; // 아이템 이름
    public int scoreValue = 10; // 점수나 증가시킬 수치

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 부딪힌 대상이 플레이어인지 확인
        if (collision.CompareTag("Player"))
        {
            // 2. 플레이어의 점수를 올리거나 인벤토리에 추가하는 로직 실행
            Debug.Log(itemName + "을 획득했습니다!");

            // 3. 먹었으니 월드에서 제거
            Destroy(gameObject);
        }
    }
}
