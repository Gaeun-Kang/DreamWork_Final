using Oculus.Interaction;
using UnityEngine;
using Oculus.Interaction.Surfaces;

public class RayRefresher : MonoBehaviour 
{
    public RayInteractor rayInteractor;

    [ContextMenu("Refresh Ray")] // 인스펙터 우클릭으로 테스트 가능
    public void RefreshRayInteractor()
    {
      
        if(rayInteractor != null)
            rayInteractor.enabled = false;

            // 한 프레임 쉬거나, 즉시 켜도 내부 라이프사이클이 갱신됩니다.
            rayInteractor.enabled = true;

            Debug.Log("XR Ray Interactor가 성공적으로 재시작되었습니다.");
     
    }


}
