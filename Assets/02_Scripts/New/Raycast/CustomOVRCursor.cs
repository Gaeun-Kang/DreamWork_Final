using UnityEngine;

public class CustomOVRCursor : OVRCursor
{

    [Header("Cursor Visuals")]
    [SerializeField] private GameObject cursorVisual; // 캔버스 조준점 이미지나 3D 구체(Sphere) 등을 연결합니다.

    private void Start()
    {
        if (cursorVisual == null)
        {
            cursorVisual = this.gameObject;
        }
    }

    // 포인터의 기본 레이 정보가 갱신될 때 호출됩니다. (선택적 구현)
    public override void SetCursorRay(Transform ray)
    {
        // 충돌하지 않았을 때 레이의 방향을 바라보게 하고 싶다면 여기에 로직을 추가합니다.
    }

    public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
    {
        // 1. 커서 비주얼을 활성화합니다.
        if (!cursorVisual.activeSelf)
        {
            cursorVisual.SetActive(true);
        }

        // 2. 커서의 위치를 충돌 지점(dest)으로 이동시킵니다.
        cursorVisual.transform.position = dest;

        // 3. 커서가 부딪힌 표면의 법선 벡터(normal)를 바라보도록 회전시킵니다.
        // 이를 통해 벽면, 바닥, 사선 어디든 커서가 표면에 딱 달라붙어 보입니다.
        if (normal != Vector3.zero)
        {
            cursorVisual.transform.rotation = Quaternion.LookRotation(normal);
        }
    }

    // 레이 포인터가 아무것도 가리키지 않거나 비활성화될 때 커서를 숨기는 기능도 추가하면 좋습니다.
    public void HideCursor()
    {
        if (cursorVisual != null)
        {
            cursorVisual.SetActive(false);
        }
    }


}
