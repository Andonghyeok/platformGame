using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    PlayerController controller;

    public Transform attackPoint;
    public float attackRange = 0.6f;
    public float damage = 10f;
    public LayerMask enemyLayer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
    }

    public void ExecuteAttack()
    {
        controller.canNextCombo = false;

        if (controller.IsGrounded)
        {
            anim.SetTrigger("Attack");
        }
        else
        {
            anim.SetTrigger("AirAttack");
            // 공중 공격 시작 시 일단 중력을 낮춰서 천천히 떨어지게 설정 (1, 2타용)
            rb.gravityScale = 0.5f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, -1f);
        }

        PerformAttack();
    }

    // [추가] 애니메이션 이벤트용: 공중 1, 2타에서 체공 시간을 늘려줌
    public void AirStay()
    {
        rb.gravityScale = 0.2f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.5f);
    }

    // [수정] 애니메이션 이벤트용: 공중 3타(내려찍기) 전용 급강하
    public void AirDownForce()
    {
        // 중력을 아주 높게 설정하고 아래로 강한 속도를 꽂음
        rb.gravityScale = controller.defaultGravity * 5f;
        rb.linearVelocity = new Vector2(0, -40f);
    }
    void PerformAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (var enemy in hits)
        {
            if (enemy.TryGetComponent(out IDamageable d))
                d.TakeDamage(damage);
        }
    }



    public void FinishAttack()
    {
        rb.gravityScale = controller.defaultGravity;
        controller.canNextCombo = false;
        controller.EndAttack();
    }
}