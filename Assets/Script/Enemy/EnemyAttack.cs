using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public Transform attackPoint; // 이제 인스펙터에서 비워둬도 됩니다.
    public float attackRange = 0.5f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.0f;
    public LayerMask targetLayers;

    private float _lastAttackTime;
    private SpriteRenderer _sr;

    // EnemyAttack.cs 수정본
    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        // 1. [클론 대응] 공격 포인트 자동 할당
        if (attackPoint == null)
        {
            attackPoint = transform.Find("AttackPoint");
            // 만약 자식이 없다면 자기 자신이라도 할당해서 에러 방지
            if (attackPoint == null) attackPoint = this.transform;
        }

        // 2. [클론 대응] 레이어를 코드로 강제 지정 (가장 중요!!)
        // "Player"라는 이름의 레이어를 타겟으로 삼습니다.
        targetLayers = 1 << LayerMask.NameToLayer("Player");
    }

    public void AttemptAttack(Transform player)
    {
        if (player == null) return;

        // 공격 쿨타임 계산
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            // 3. [순간이동 방지] 공격 시점에만 방향과 포인트 위치를 딱 맞춤
            bool shouldFlip = player.position.x < transform.position.x;
            _sr.flipX = shouldFlip;

            if (attackPoint != null && attackPoint != transform)
            {
                float offsetX = 0.7f; // 이 값은 유니티에서 보면서 조절하세요.
                attackPoint.localPosition = new Vector3(shouldFlip ? -offsetX : offsetX, 0, 0);
            }

            PerformAttack();
            _lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        if (attackPoint == null) return;

        // [수정] 실제 타격 판정 범위를 설정된 attackRange보다 0.2f 정도 더 넓게 잡습니다.
        // 이렇게 하면 몬스터가 살짝 멀리서 멈춰도 플레이어가 사거리에 걸립니다.
        float finalRange = attackRange + 0.2f;

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, finalRange, targetLayers);

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D collider in hitTargets)
            {
                IDamageable target = collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(attackDamage);
                }
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