using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMove : MonoBehaviour
{
    [Header("참조")]
    public TrailRenderer trailRenderer;
    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    [Header("기본 이동 구현")]
    public float MoveSpeed;
    private float moveInput;

    [Header("점프 구현")]
    public float minJumpForce;
    public float maxJumpForce;
    private bool isGrounded;
    private float JumpChargingTime;
    private float maxChargingTime = 1.5f;
    private bool isJumpCharging;




    [Header("대시 구현")]
    public float maxDashSpeed;
    public float minDashSpeed;
    public float dashDuration;
    bool isDashing;
    bool canDash = true;
    private float maxDashChargeTime = 1.5f;
    private float dashChargeTime;
    private bool isDashCharging;

    private Vector2 _currentDirection;
    private float _lastDirection;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (trailRenderer != null) trailRenderer.emitting = false;
        
    }
    private void FixedUpdate()
    {
        if (isDashing) return;


        isGrounded = Physics2D.Raycast(rigid.position, Vector2.down, 0.7f, LayerMask.GetMask("Platform"));

        ApplyMove();
    }
    private void Update()
    {
        ChargingTimer();
        UpdateAnimations();
    }
    private void UpdateAnimations()
    {
        // 1. 달리기 (가로 속도)
        anim.SetFloat("Speed", Mathf.Abs(rigid.linearVelocity.x));

        // 2. 바닥 체크
        anim.SetBool("IsGrounded", isGrounded);

        // 3. 점프와 낙하 (세로 속도)
        // rigid.linearVelocity.y 값이 양수면 상승(Jump), 음수면 하강(Fall)입니다.
        anim.SetFloat("yVelocity", rigid.linearVelocity.y);

        // 4. 대시 상태 전달
        anim.SetBool("IsDashing", isDashing);
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        float xInput = context.ReadValue<Vector2>().x;
        _currentDirection.x =xInput;

        if (xInput != 0)
        {
            _lastDirection = xInput > 0 ? 1f : -1f;
            spriteRenderer.flipX = _currentDirection.x < 0;
        }
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded && !isDashing)
        {
            isJumpCharging = true;
            JumpChargingTime = 0f;
            Debug.Log("점프 차징 시작...");
        }
        if (context.canceled && isJumpCharging)
        {
            _currentDirection.y = 1;
            JumpWithCharging();
        }

    }
    public void OnDash(InputAction.CallbackContext context)
    {

        if (context.started && canDash && !isDashing)
        {
            isDashCharging = true;
            dashChargeTime = 0f;

            Debug.Log(" 대시 차징 시작... 기를 모으는 중!");
        }


        if (context.canceled && isDashCharging)
        {

            Debug.Log($" 대시 발사! (차징 시간: {dashChargeTime:F2}초)");
            StartCoroutine(ChargedDashRoutine());
        }
    }
    private void ChargingTimer()
    {
        if (isJumpCharging)
        {
            JumpChargingTime += Time.deltaTime;
            JumpChargingTime = Mathf.Clamp(JumpChargingTime, 0, maxChargingTime);
        }
        if (isDashCharging)
        {
            dashChargeTime += Time.deltaTime;
            dashChargeTime = Mathf.Clamp(dashChargeTime, 0, maxDashChargeTime);
        }
    }




    private void ApplyMove()
    {
        if (isJumpCharging || isDashCharging)
        {
            if (isJumpCharging) rigid.linearVelocity = Vector2.zero;
            return;
        }
        if (isDashing || isDashCharging)
        {
            if (isDashCharging) rigid.linearVelocity = new Vector2(_currentDirection.x * MoveSpeed, rigid.linearVelocity.y);
            return;
        }
        if (_currentDirection.x == 0)
        {
            rigid.linearVelocity = new Vector2(0, rigid.linearVelocity.y);
        }
        else
        {
            rigid.AddForce(Vector2.right * _currentDirection* MoveSpeed, ForceMode2D.Impulse);
        }

        float clampedX = Mathf.Clamp(rigid.linearVelocity.x, -MoveSpeed, MoveSpeed);
        rigid.linearVelocity = new Vector2(clampedX, rigid.linearVelocity.y);
    }
    private void JumpWithCharging()
    {
        isJumpCharging = false;

        float chargeRatio = JumpChargingTime / maxChargingTime;
        float finalJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargeRatio);

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, finalJumpForce);

        Debug.Log($"점프! 차징시간: {JumpChargingTime:F2}s, 적용된 힘: {finalJumpForce:F2}");

        JumpChargingTime = 0f; 
    }


    IEnumerator ChargedDashRoutine()
    {
        isDashCharging = false;
        canDash = false;
        isDashing = true;
        // 여기서 isDashing이 true가 되면, UpdateAnimations에서 자동으로 
        // 애니메이터의 isDashing 파라미터를 true로 만듭니다.

        float chargeRatio = dashChargeTime / maxDashChargeTime;
        float currentDashSpeed = Mathf.Lerp(minDashSpeed, maxDashSpeed, chargeRatio);

        float originalGravity = rigid.gravityScale;
        rigid.gravityScale = 0f;
        rigid.linearVelocity = new Vector2(currentDashSpeed * _lastDirection, 0f);

        if (trailRenderer != null) trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashDuration);

        if (trailRenderer != null) trailRenderer.emitting = false;

        rigid.gravityScale = 3.0f;
        isDashing = false; // 대시 종료 -> 애니메이션도 다시 기본 상태로 돌아감

        yield return new WaitForSeconds(2f);
        canDash = true;
    }
}