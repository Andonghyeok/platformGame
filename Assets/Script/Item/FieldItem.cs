using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public int itemID;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EffectHandler handler = collision.GetComponent<EffectHandler>();

            if (handler != null)
            {
                Debug.Log($"[FieldItem] 플레이어 감지! ID {this.itemID} 적용 시도");
                handler.ApplyEffect(this.itemID);
            }
            else
            {
                Debug.LogError("[FieldItem] 부딪힌 대상에게 EffectHandler가 없습니다! 스크립트를 확인하세요.");
            }

            Destroy(gameObject);
        }
    }
}