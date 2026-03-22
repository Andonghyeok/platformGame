using UnityEngine;
using System.Collections;

public class BoxOpen : MonoBehaviour, IDamageable
{
    [Header("드랍 설정")]
    // 이제 프리팹 대신 Pool에서 사용할 '이름(Key)'을 적습니다. (예: "Coin", "Gem")
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

        // 2. [핵심] Dictionary 풀에서 아이템 꺼내기
        if (ItemObjectPool.Instance != null)
        {
            // 인스펙터에 적어준 itemKey("Coin" 등)를 넘겨줍니다.
            GameObject item = ItemObjectPool.Instance.GetItem(itemKey);

            if (item != null)
            {
                item.transform.position = dropPoint.position;
                item.transform.rotation = Quaternion.identity;

                // 물리 초기화 및 발사
                Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    Vector2 jumpDir = new Vector2(Random.Range(-1f, 1f), 3f);
                    rb.AddForce(jumpDir, ForceMode2D.Impulse);
                }
            }
        }

        // 3. 상자 페이드 아웃 연출
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / 0.5f);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 4. 상자 파괴
        Destroy(gameObject);
    }
}