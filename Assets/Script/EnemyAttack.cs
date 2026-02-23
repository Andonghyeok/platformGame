using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.0f; 
    public LayerMask targetLayers;     

    private float _lastAttackTime;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void AttemptAttack(Transform player)
    {
        // 공격 중 플레이어 방향 쳐다보기
        if (player != null)
        {
            _sr.flipX = player.position.x < transform.position.x;
        }

        // 쿨타임 체크 후 공격
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            PerformAttack();
            _lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        Debug.Log("적군 자동 공격 실행!");
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayers);

        foreach (Collider2D collider in hitTargets)
        {
            IDamageable target = collider.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}