using UnityEngine;

public interface IState
{
    void OnEnterState();

    void OnUpdateState();

    void OnExitState();

}
public class PatrolState : IState
{
    private Enemy _enemy;
    private int _direction = 1;

    public PatrolState(Enemy enemy)
    {
        _enemy = enemy;
    }

    public void OnEnterState()
    {
        Debug.Log("순찰/복귀 상태 진입");
    }

    public void OnUpdateState()
    {
        //추격 체크 (플레이어가 감지 범위 안에 오면 즉시 ChaseState로 변환)
        if (_enemy.playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(_enemy.transform.position, _enemy.playerTransform.position);
            if (distanceToPlayer <= _enemy._detectRange)
            {
                _enemy.ChangeState(new ChaseState(_enemy));
                return;
            }
        }

        // 현재 내 위치와 시작점의 거리 차이 계산
        float distFromStart = _enemy._startPosition.x - _enemy.transform.position.x;

        // [복귀 로직] 집(시작점)에서 너무 멀리 떨어져 있다면? (추격 끝난 직후 등)
        // 순찰 범위(_patrolRange)보다 밖에 있다면 일단 집 방향으로 걷습니다.
        if (Mathf.Abs(distFromStart) > _enemy._patrolRange + 0.1f)
        {
            float returnDir = distFromStart > 0 ? 1 : -1;
            Move(returnDir);
        }
        // [순찰 로직] 집 근처에 도착했거나 이미 범위 안이라면 평소처럼 순찰합니다.
        else
        {
            // 순찰 경계선 계산
            float leftBoundary = _enemy._startPosition.x - _enemy._patrolRange;
            float rightBoundary = _enemy._startPosition.x + _enemy._patrolRange;

            // 끝지점에 닿으면 방향 전환
            if (_enemy.transform.position.x >= rightBoundary) _direction = -1;
            else if (_enemy.transform.position.x <= leftBoundary) _direction = 1;

            Move(_direction);
        }
    }

    // 이동과 Flip(뒤돌기)을 한 번에 처리하는 함수
    private void Move(float dir)
    {
        _enemy.transform.Translate(Vector2.right * dir * _enemy._moveSpeed * Time.deltaTime);

        if (_enemy.spriteRenderer != null)
        {
            _enemy.spriteRenderer.flipX = (dir < 0);
        }
    }

    public void OnExitState() { }
}
public class ChaseState : IState
{
    private Enemy _enemy;
    private EnemyAttack _attack;

    public ChaseState(Enemy enemy)
    {
        _enemy = enemy;
        // 성능을 위해 미리 캐싱해둡니다.
        _attack = _enemy.GetComponent<EnemyAttack>();
    }

    public void OnEnterState()
    {
        Debug.Log("--- 추격 상태 진입 ---");
    }

    public void OnUpdateState()
    {
        if (_enemy.playerTransform == null)
        {
            _enemy.ChangeState(new PatrolState(_enemy));
            return;
        }

        float distance = Vector2.Distance(_enemy.transform.position, _enemy.playerTransform.position);

        // [중요] 공격 범위 안이라면 "공격만" 하고 함수를 종료(return)합니다.
        if (distance <= _enemy._attackRange)
        {
            if (_attack != null)
            {
                _attack.AttemptAttack(_enemy.playerTransform);
            }
            return; // 여기서 리턴하여 아래의 이동 로직(Translate)이 실행되지 않게 합니다.
        }

        // [공격 범위 밖일 때만 이동]
        MoveTowardsPlayer();

        // 추격 포기 거리 체크
        if (distance > _enemy._detectRange * 1.5f)
        {
            _enemy.ChangeState(new PatrolState(_enemy));
        }
    }

    private void MoveTowardsPlayer()
    {
        float dirX = _enemy.playerTransform.position.x - _enemy.transform.position.x;
        float moveDir = dirX > 0 ? 1 : -1;

        // 방향 전환
        if (_enemy.spriteRenderer != null)
        {
            _enemy.spriteRenderer.flipX = moveDir < 0;
        }

        // 실제 이동
        _enemy.transform.Translate(Vector2.right * moveDir * _enemy._chaseSpeed * Time.deltaTime);
    }

    public void OnExitState()
    {
        Debug.Log("--- 추격 상태 종료 ---");
    }
}

