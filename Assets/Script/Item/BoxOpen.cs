using UnityEngine;
using System.Collections;

public class BoxOpen : MonoBehaviour, IDamageable
{
    [Header("드랍 설정")]
    [SerializeField] private string itemKey = "Coin";
    [SerializeField] private Transform dropPoint;

    [Header("상태")]
    [SerializeField] private float _maxHealth = 50f;
    private float _currentHealth;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator anim;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        originalColor = spriteRenderer.color;

        if (dropPoint == null) dropPoint = transform;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        _currentHealth -= damage;

        // 피격 시 반짝이는 연출
        StopAllCoroutines();
        StartCoroutine(HitFlashRoutine());

        if (_currentHealth <= 0)
        {
            isDead = true;
            StartCoroutine(DieRoutine());
        }
    }

    IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator DieRoutine()
    {
        // 1. 애니메이션 실행
        if (anim != null) anim.SetTrigger("Open");
        yield return new WaitForSeconds(0.5f);

        // 2. 통합 풀 매니저에서 아이템 꺼내기
        if (ObjectPoolManager.Instance != null)
        {
            GameObject item = ObjectPoolManager.Instance.GetObject(itemKey);

            if (item != null)
            {
                // 위치 초기화 (상자의 위치보다 약간 위에서 소환하면 바닥에 박히는 걸 방지합니다)
                item.transform.position = dropPoint.position + Vector3.up * 0.2f;
                item.transform.rotation = Quaternion.identity;

                Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // [필수] 이전 물리 상태 완전 초기화
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;

                    // [수정] 튀어오르는 힘 강화
                    // X: 좌우 랜덤 (-2 ~ 2)
                    // Y: 위로 솟구치는 힘 (5 ~ 7 정도로 대폭 상향)
                    float jumpPower = Random.Range(3f, 5f);
                    Vector2 jumpDir = new Vector2(Random.Range(-2f, 2f), jumpPower);

                    // ForceMode2D.Impulse는 순간적인 폭발력을 줍니다.
                    rb.AddForce(jumpDir, ForceMode2D.Impulse);

                    Debug.Log($"[아이템 드랍] {itemKey} 발사! 힘: {jumpDir}");
                }
            }
        }

        // 3. 상자 페이드 아웃 및 파괴 로직 (기존과 동일)
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / 0.5f);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}