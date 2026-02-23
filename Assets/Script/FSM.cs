using UnityEngine;

public class FSM
{
    private IState _currentState;
    public FSM(IState initialState)
    {
        TransitionTo(initialState);
    }
    public void TransitionTo(IState newState)
    {
        if (newState == null) return;

        //현재 상태가 있다면, 그 상태를 끝내고 나옴
        _currentState?.OnExitState();

        // 새로운 상태로 교체
        _currentState = newState;

        //새로운 상태에 들어왔음을 알림
        _currentState.OnEnterState();
    }
    // 매 프레임 현재 상태의 업데이트를 실행
    public void OnUpdate()
    {
        _currentState?.OnUpdateState();
    }
}
