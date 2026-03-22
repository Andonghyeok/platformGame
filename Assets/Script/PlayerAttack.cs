using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public Transform attackPoint;       // 공격 위치 
    public float attackRange = 0.5f;    // 공격 반경
    public float attackDamage = 10f;    // 공격 데미지
    public float attackCooldown = 0.3f; // 공격 속도
    public LayerMask enemyLayers;        // 적 레이어
    private Animator anim;

    private float _nextAttackTime = 0f;
    private PlayerMove _playerMove;  

    private void Awake()
    {
        _playerMove = GetComponent<PlayerMove>();
        anim = GetComponent<Animator>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started && Time.time >= _nextAttackTime)
        {
            PerformAttack();
            _nextAttackTime = Time.time + attackCooldown;
            anim.SetTrigger("Attack");
        }
    }

    private void PerformAttack()
    {
        Debug.Log(" 공격!");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D collider in hitEnemies) 
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}