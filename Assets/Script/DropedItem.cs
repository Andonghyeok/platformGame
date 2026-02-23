using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    public string itemKey; // 인스펙터에서 "Coin" 혹은 "Gem" 등으로 설정

    public void OnCollect() // 플레이어가 먹었을 때 호출
    {
        ItemObjectPool.Instance.ReleaseItem(itemKey, gameObject);
    }

    // 일정 시간 후 자동 반납 예시
    private void OnEnable()
    {
        Invoke(nameof(AutoRelease), 5f);
    }

    private void AutoRelease()
    {
        ItemObjectPool.Instance.ReleaseItem(itemKey, gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}