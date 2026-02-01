using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMove : MonoBehaviour
{
    public TrailRenderer trailRenderer;

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;

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
        if (isJumpCharging || isJumpCharging)
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

        float chargeRatio = dashChargeTime / maxDashChargeTime;
        float currentDashSpeed = Mathf.Lerp(minDashSpeed, maxDashSpeed, chargeRatio);

        Debug.Log($" 최종 대시 속도: {currentDashSpeed:F1}");

        float originalGravity = rigid.gravityScale;
        rigid.gravityScale = 0f;
        

        rigid.linearVelocity = new Vector2(currentDashSpeed * _lastDirection, 0f);

        if (trailRenderer != null) trailRenderer.emitting = true;

        yield return new WaitForSeconds(dashDuration);

        if (trailRenderer != null) trailRenderer.emitting = false;

        rigid.gravityScale = 3.0f;
        isDashing = false;

        Debug.Log(" 대시 종료, 쿨타임 시작");
        yield return new WaitForSeconds(2f);

        canDash = true;
        Debug.Log("대시 재사용 가능!");
    }
}