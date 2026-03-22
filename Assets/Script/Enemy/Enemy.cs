using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets; // [추가] Addressables 메모리 해제를 위해 필요

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
    private Color _originalColor = Color.white; // 기본 색상 저장

    // FSM 관련
    private FSM _fsm;

    private void Awake()
    {
        // 초기 체력 설정 (나중에 Spawner가 다시 덮어쓸 수 있음)
        _currentHealth = _maxHealth;
        _startPosition = transform.position;
  

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        _enemyAttack = GetComponent<EnemyAttack>();

        // [클론 대응] 태그가 "Player"인 오브젝트 자동 찾기
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private MonsterData _myStat;
    private SpawnerData _mySpawnData;
    private MonsterSpawner _mySpawner;
    private bool _isDead = false;

    // 소환될 때 정보 받기
    public void Setup(MonsterData stat, SpawnerData sData, MonsterSpawner spawner)
    {
        // 1. 색상 초기화 (빨간색에서 다시 원래대로)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = _originalColor;
        }

        // 2. 물리 및 상태 초기화
        _isDead = false;
        _currentHealth = stat.maxHp;

        // 3. 애니메이션 초기화 (죽는 애니메이션 등이 재생 중일 수 있음)
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.Rebind(); // 애니메이터를 처음 상태로 리셋
            anim.Update(0f);
        }

        // 기타 스패너 정보 저장...
        _myStat = stat;
        _mySpawnData = sData;
        _mySpawner = spawner;
    }

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // 1. 스패너에게 죽었다고 알림 (재생성 예약)
        if (_mySpawner != null)
        {
            _mySpawner.OnMonsterDead(_mySpawnData);
        }

        // 2. 풀에 반납 (Destroy 대신 사용)
        string cleanKey = _myStat.prefabKey.Trim();
        ObjectPoolManager.Instance.ReleaseObject(cleanKey, gameObject);
    }


public Vector3 GetStartPosition()
    {
        return _startPosition;
    }

    private void Start()
    {
        _fsm = new FSM(new PatrolState(this));
    }

    private void Update()
    {
        _fsm?.OnUpdate();

        // 2D 회전 고정 (Z축 튀는 현상 방지)
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void ChangeState(IState newState)
    {
        _fsm?.TransitionTo(newState);
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        StopAllCoroutines();
        StartCoroutine(HitEffectRoutine());

        if (_currentHealth <= 0) Die();
    }



    private void OnDrawGizmos()
    {
        Vector3 center = Application.isPlaying ? _startPosition : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + Vector3.left * _patrolRange, center + Vector3.right * _patrolRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }

    IEnumerator HitEffectRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }
}