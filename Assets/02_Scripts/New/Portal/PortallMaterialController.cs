using UnityEngine;

public class PortallMaterialController : MonoBehaviour 
{
    [Header("Portal Material 설정")]
    [SerializeField] private string texturePropertyName = "_MainTex";

    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _portalMaterial;  //Ray 선택시 Material 대체 

    private Texture2D _originalTexture;
    [SerializeField]private ImageSetData _currentImageSet;

    public ImageSetData GetCurrentImageSet() => _currentImageSet;
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originalTexture = _portalMaterial.GetTexture(texturePropertyName) as Texture2D;
    }


    /// Dream Sphere에서 선택된 ImageSet에 맞게 Relate Image로 변경

    public void ApplyRelateImage(ImageSetData imageSet)
    {
        if (imageSet == null)
        {
            Debug.LogWarning("[PortalMaterialController] ImageSetData가 null입니다.");
            return;
        }

        if (imageSet.relateImage == null)
        {
            Debug.LogWarning($"[PortalMaterialController] Set [{imageSet.setID}]의 Relate Image가 없습니다.");
            return;
        }

        _currentImageSet = imageSet;
        _renderer.material = _portalMaterial;
        _portalMaterial.SetTexture(texturePropertyName, imageSet.relateImage);
        Debug.Log($"[PortalMaterialController] Portal → Relate Image [{imageSet.setID}] 적용 완료");
    }



    /// 포탈 Material을 원래 텍스쳐로 복원

    public void ResetToOriginal()
    {
        if (_originalTexture != null)
            _portalMaterial.SetTexture(texturePropertyName, _originalTexture);
    }

   /* private void OnDestroy()
    {
        // 인스턴스 Material 메모리 해제
        if (_portalMaterial != null)
            Destroy(_portalMaterial);
    }
   */
}
