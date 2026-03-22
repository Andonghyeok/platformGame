using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;                // 카메라를 참조하기 위한 공개 변수
    public Transform followTarget;    // 팔로우할 대상을 참조하기 위한 공개 변수

    Vector2 startingPosition;        // 패럴랙스 게임 오브젝트의 시작 위치를 저장하는 벡터
    float startingZ;                  // 패럴랙스 게임 오브젝트의 시작 Z 값

    // 카메라 이동량을 계산하여 반환하는 익명 속성 (Property)
    Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startingPosition;

    // 대상과 패럴랙스 오브젝트 간의 Z 거리를 반환하는 익명 속성 (Property)
    float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;

    // 클리핑 플레인 (클리핑 평면)을 계산하여 반환하는 익명 속성 (Property)
    float clippingPlane => (cam.transform.position.z + (zDistanceFromTarget) > 0 ? cam.farClipPlane : cam.nearClipPlane);

    // 패럴랙스 계수를 계산하여 반환하는 익명 속성 (Property)
    float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;

    // Start 함수는 첫 번째 프레임 이전에 호출됩니다.
    void Start()
    {
        // 패럴랙스 게임 오브젝트의 시작 위치와 Z 값을 저장합니다.
        startingPosition = transform.position;
        startingZ = transform.localPosition.z;
    }

    // Update 함수는 매 프레임마다 호출됩니다.
    void Update()
    {
        // parallaxFactor가 0이 되는 것을 방지 (에러 방지)
        float safeFactor = Mathf.Max(0.001f, parallaxFactor);

        // X축으로만 패럴랙스 효과를 적용하고, Y축은 시작 위치로 고정합니다.
        float newX = startingPosition.x + camMoveSinceStart.x / safeFactor;
        float newY = startingPosition.y; // Y축은 고정

        // 패럴랙스 오브젝트의 위치를 새로 계산된 위치로 업데이트하며 Z 값은 시작 Z 값으로 유지합니다.
        transform.position = new Vector3(newX, newY, startingZ);
    }
}