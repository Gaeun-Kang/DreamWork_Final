using UnityEngine;
using Unity.Mathematics;

public class SplineConnectToEgo : MonoBehaviour
{
    public Transform egoSphere;
    public Transform dreamSphere; //Spline이 향해야하는 방향 

    private Quaternion initialRotation;
    private Vector3 initialDirection;

    void Awake()
    {

        //if (egoSphere == null) return;

        // 처음 배치된 상태의 회전값 저장
        initialRotation = transform.rotation;

        // 처음 EgoSphere -> DreamSphere 방향 저장
        initialDirection = dreamSphere.position - egoSphere.position;

        if (initialDirection.sqrMagnitude > 0.0001f)
            initialDirection.Normalize();
    }

    private void OnEnable()
    {
        DreamSphereManager.Instance.OnSphereSelected += SetdreamSphere;
    }

    private void OnDisable()
    {
        DreamSphereManager.Instance.OnSphereSelected -= SetdreamSphere;
    }

    private void SetdreamSphere(Transform sphererTransform)
    {
        dreamSphere = sphererTransform;
        Debug.Log("Dream Sphere 선택 완료");

        if (egoSphere != null && dreamSphere != null)
        {
            // 타겟이 설정되는 순간의 상태를 '초기 상태'로 갱신합니다.
            initialRotation = transform.rotation;
            initialDirection = (dreamSphere.position - egoSphere.position).normalized;
        }

    }


    void LateUpdate()
    {
        if (egoSphere == null || dreamSphere == null) return;

        // 1. Spline의 pivot 위치를 EgoSphere에 맞춤
        transform.position = egoSphere.position;

        // 2. 현재 EgoSphere -> DreamSphere 방향
        Vector3 currentDirection = dreamSphere.position - egoSphere.position;

        if (currentDirection.sqrMagnitude < 0.0001f) return;

        currentDirection.Normalize();

        // 3. 처음 방향에서 현재 방향으로 얼마나 달라졌는지만 계산
        Quaternion deltaRotation = Quaternion.FromToRotation(initialDirection, currentDirection);

        // 4. 원래 Spline의 회전값을 보존한 채 기울기만 보정
        transform.rotation = deltaRotation * initialRotation;
    }
}