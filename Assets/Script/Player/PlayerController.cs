using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    InputBuffer input;
    PlayerCombat combat;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    // [추가] 스텟 관리 클래스 참조
    PlayerStatModifier stats;

    public PlayerState state = PlayerState.Idle;
    public bool canNextCombo = false;

    [Header("GroundCheck")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;
    public bool IsGrounded;

    // [수정] moveSpeed, maxJump 등을 삭제하거나 stats에서 가져올 준비를 합니다.
    // 만약 점프나 대시도 보정하고 싶다면 StatModifier에 추가하면 됩니다.
    [Header("Jump & Dash")]
    public float maxJump = 15; // 이것도 원하면 Stat으로 만들 수 있습니다.
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    bool canDash = true;

    [HideInInspector] public float defaultGravity;
    float lastDir = 1;

    void Awake()
    {
        input = GetComponent<InputBuffer>();
        combat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // [추가] 컴포넌트 가져오기
        stats = GetComponent<PlayerStatModifier>();

        defaultGravity = rb.gravityScale;
    }

    void Update()
    {
        UpdateGround();
        UpdateDirection();
        ProcessInput();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // 공격/대시가 아니면 중력 복구
        if (state != PlayerState.Attacking && state != PlayerState.Dashing)
        {
            rb.gravityScale = defaultGravity;
        }

        if (state == PlayerState.Attacking)
        {
            // 공중 공격 중 바닥에 닿으면 즉시 종료
            if (IsGrounded)
            {
                // 내려찍기(Y속도가 매우 빠를 때) 중 착지라면 이펙트나 진동 추가 가능
                if (rb.linearVelocity.y < -20f)
                {
                    // 여기에 카메라 흔들림 등 추가 가능
                }

                rb.linearVelocity = Vector2.zero; // 착지 시 멈춤
                EndAttack();
            }
            return;
        }

        if (state == PlayerState.Dashing) return;

        Move();
    }

    void UpdateGround() => IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

    void UpdateDirection()
    {
        float x = input.MoveInput.x;
        if (Mathf.Abs(x) > 0.1f)
        {
            lastDir = Mathf.Sign(x);
            sr.flipX = x < 0;
        }
    }

    void Move()
    {
        // [수정] 이제 stats.MoveSpeed.Value를 사용하여 최종 계산된 속도를 가져옵니다.
        float x = input.MoveInput.x * stats.MoveSpeed.Value;
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);

        if (state == PlayerState.Idle || state == PlayerState.Moving)
            state = Mathf.Abs(x) > 0.1f ? PlayerState.Moving : PlayerState.Idle;
    }

    void ProcessInput()
    {
        var next = input.Peek();
        if (!next.HasValue) return;

        if (state == PlayerState.Attacking && !canNextCombo && next.Value.type == InputType.Attack)
            return;

        if (!CanExecute(next.Value)) return;

        Execute(input.Pop().Value);
    }

    bool CanExecute(InputData data)
    {
        if (state == PlayerState.Stunned) return false;

        switch (data.type)
        {
            case InputType.Attack:
                if (state == PlayerState.Dashing) return false;

                if (state != PlayerState.Attacking) return true;

                return canNextCombo;

            case InputType.Jump:
                if (data.phase == InputPhase.Started) return IsGrounded;
                return state == PlayerState.JumpCharging;

            case InputType.Dash:
                if (data.phase == InputPhase.Started) return canDash;
                return state == PlayerState.DashCharging;
        }
        return true;
    }

    void Execute(InputData cmd)
    {
        switch (cmd.type)
        {
            case InputType.Attack:
                state = PlayerState.Attacking;
                combat.ExecuteAttack();
                break;
            case InputType.Jump:
                if (cmd.phase == InputPhase.Started) state = PlayerState.JumpCharging;
                else Jump();
                break;
            case InputType.Dash:
                if (cmd.phase == InputPhase.Started) state = PlayerState.DashCharging;
                else StartCoroutine(Dash());
                break;
        }
    }

    public void SetComboWindow(int active)
    {
        canNextCombo = (active == 1);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxJump);
        state = PlayerState.Idle;
    }

    IEnumerator Dash()
    {
        state = PlayerState.Dashing;
        canDash = false;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(lastDir * dashSpeed, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = defaultGravity;
        state = PlayerState.Idle;
        yield return new WaitForSeconds(1f);
        canDash = true;
    }

    public void EndAttack() => state = PlayerState.Idle;

    void UpdateAnimations()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("IsGrounded", IsGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("Dash", state == PlayerState.Dashing);
    }

    public void TakeDamage(float damage)
    {
        // [수정] 이제 stats에 있는 CurrentHealth를 깎습니다.
        stats.CurrentHealth -= damage;

        if (stats.CurrentHealth <= 0)
        {
            Die(); // 사망 로직 호출 (필요시 구현)
            return;
        }

        state = PlayerState.Stunned;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Hit");
        StartCoroutine(Hit());
    }

    IEnumerator Hit()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sr.color = Color.white;
        state = PlayerState.Idle;
    }

    void Die()
    {
        state = PlayerState.Dead;

        // 1. 물리 정지 및 충돌 감지 해제
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // 더 이상 물리 연산 안 함 (낙하 방지라면 이것만)
                              // 혹은 rb.bodyType = RigidbodyType2D.Static; // 완전히 고정

        // 2. 애니메이션 실행
        anim.SetTrigger("Die");

        // 3. (선택) 사망 후 레이어 변경 (적들이 시체를 무시하도록)
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        Debug.Log("플레이어가 사망했습니다. 게임 오버 UI를 띄웁니다.");

        // 4. 게임 오버 UI 호출 등 (예: GameManager.Instance.GameOver())
    }
}

