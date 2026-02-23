using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [Header("능력치")]
    public float _maxHealth = 50.0f;
    public float _currentHealth;
    [Header("피격 연출 ")]
    private SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;

    private void Awake()
    {
        _currentHealth = _maxHealth; 
        spriteRenderer = GetComponent<SpriteRenderer>(); 
    }
    public void TakeDamage(float damage)
    {

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name}이(가) {damage}의 데미지를 입음! 남은 체력: {_currentHealth}");
        StopAllCoroutines();
        StartCoroutine(HitEffectRoutine());

        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    private System.Collections.IEnumerator HitEffectRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        Destroy(gameObject);
    }
}
