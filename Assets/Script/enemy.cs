using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("능력치")]
    public float _maxHealth = 50f;
    public float _currentHealth;

    [Header("순찰 및 추격 설정")]
    public float _moveSpeed = 2.0f;
    public float _chaseSpeed = 4.0f;
    public float _patrolRange = 3.0f;
    public float _detectRange = 5.0f;
    public float _attackRange = 1.0f;

    [Header("참조")]
    public Transform playerTransform;
    public SpriteRenderer spriteRenderer; 
    private EnemyAttack _enemyAttack;
    public Vector3 _startPosition;

    // FSM 관련
    private FSM _fsm;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _startPosition = transform.position;

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        _enemyAttack = GetComponent<EnemyAttack>();
    }
    public Vector3 GetStartPosition()
    {
        return _startPosition;
    }

    private void Start()
    {
        // FSM 초기화 
        _fsm = new FSM(new PatrolState(this));
    }

    private void Update()
    {
        _fsm.OnUpdate();
    }

    // 상태 전환용 헬퍼 메서드
    public void ChangeState(IState newState)
    {
        _fsm.TransitionTo(newState);
    }

    // 데미지 인터페이스 구현 (기존 로직 유지)
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    private void OnDrawGizmos()
    {
        // 1. 순찰 범위 (빨간색 선)
        // 게임이 실행 중이 아닐 때는 transform.position을 기준으로 그림
        Vector3 center = Application.isPlaying ? _startPosition : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + Vector3.left * _patrolRange, center + Vector3.right * _patrolRange);

        // 2. 감지 범위 (노란색 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRange);

        // 3. 공격 범위 (초록색 원)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}