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
    private Coroutine hitFlashCoroutine;

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

        if (hitFlashCoroutine != null) StopCoroutine(hitFlashCoroutine);
        hitFlashCoroutine = StartCoroutine(HitFlashRoutine());

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


        if (ObjectPoolManager.Instance != null)
        {
            GameObject item = ObjectPoolManager.Instance.GetObject(itemKey);

            if (item != null)
            {
                item.transform.position = dropPoint.position + Vector3.up * 0.2f;
                item.transform.rotation = Quaternion.identity;

                Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
                if (rb != null)
                { 
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;


                    float jumpPower = Random.Range(3f, 5f);
                    Vector2 jumpDir = new Vector2(Random.Range(-2f, 2f), jumpPower);

                    rb.AddForce(jumpDir, ForceMode2D.Impulse);

                    Debug.Log($"[아이템 드랍] {itemKey} 발사! 힘: {jumpDir}");
                }
            }
        }


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