using UnityEngine;

//RayCast로 선택할 GameObject에 부착 

public class SelectableObject : MonoBehaviour
{

    [Header("Registry Reference")]
    [SerializeField] private ImageSetting imageSetting;

    [Header("Override (비워두면 Renderer Material 텍스쳐 자동 감지)")]
     [SerializeField] private ImageSetData overrideImageSet;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public ImageSetData GetImageSet()
    {
        if (overrideImageSet != null)
            return overrideImageSet;

        if (imageSetting == null)
        {
            Debug.LogWarning($"[SelectableObject] ImageSetting이 없습니다: {gameObject.name}");
            return null;
        }

        // Material의 메인 텍스쳐를 기반으로 ImageSet에서 연관 탐색
        var mat = _renderer.sharedMaterial;
        if (mat == null) return null;

        Texture2D mainTex = mat.mainTexture as Texture2D;
        if (mainTex == null) return null;

        return imageSetting.GetByMainTexture(mainTex);
    }
   
}
