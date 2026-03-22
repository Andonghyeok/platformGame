using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour, IDamageable
{
    InputBuffer input;
    PlayerCombat combat;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    public PlayerState state = PlayerState.Idle;
    public bool canNextCombo = false; 

    [Header("GroundCheck")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;
    public bool IsGrounded;

    [Header("Movement")]
    public float moveSpeed = 8f;
    [Header("Jump")]
    public float maxJump = 15;
    [Header("Dash")]
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
        if (state == PlayerState.Dashing || state == PlayerState.Attacking)
        {
            if (IsGrounded)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return; 
        }

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
        float x = input.MoveInput.x * moveSpeed;
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
}