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
        // 공격이 시작되면 다음 콤보 입력을 잠시 막아 중복 소모를 방지합니다.
        controller.canNextCombo = false;

        if (controller.IsGrounded)
        {
            anim.SetTrigger("Attack");
        }
        else
        {
            anim.SetTrigger("AirAttack");
            // 공중 1, 2타 체공 처리
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, 0f);
        }

        PerformAttack();
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

    // [애니메이션 이벤트용] Air_Attack3 시작 시 호출하여 급강하 시킴
    public void AirDownForce()
    {
        rb.gravityScale = controller.defaultGravity * 3f;
        rb.linearVelocity = new Vector2(0, -50f);
    }

    // [애니메이션 이벤트용] 모든 공격 끝에 호출
    public void FinishAttack()
    {
        rb.gravityScale = controller.defaultGravity;
        controller.canNextCombo = false;
        controller.EndAttack();
    }
}