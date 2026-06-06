using UnityEngine;

//RayCast로 선택할 GameObject에 부착 

public class SelectableObject : MonoBehaviour
{

    [Header("Registry Reference")]
    [SerializeField] private ImageSetting imageSetting;

    [Header("현재 할당된 Globe Index (읽기 전용 / 디버그)")]
    [SerializeField, HideInInspector] private int _globeIndex = -1;

    private Renderer _renderer;


    public void SetGlobeIndex(int index)
    {
        _globeIndex = index;
    }

    public void SetImageSetting(ImageSetting newimageSetting)
    {
        imageSetting = newimageSetting;
    }


    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public ImageSetData GetImageSet()
    {
        if (imageSetting == null)
        {
            Debug.LogWarning($"[SelectableObject] '{gameObject.name}' 에 Registry가 없습니다.", this);
            return null;
        }

        if (_globeIndex < 0)
        {
            Debug.LogWarning(
                $"[SelectableObject] '{gameObject.name}' 의 GlobeIndex가 설정되지 않았습니다. " +
                "오브젝트 생성 코드에서 SetGlobeIndex(index)를 호출했는지 확인하세요.", this);
            return null;
        }

        var result = imageSetting.GetByIndex(_globeIndex);
        if (result == null)
            Debug.LogWarning(
                $"[SelectableObject] Registry index [{_globeIndex}] 에 해당하는 ImageSetData가 없습니다. " +
                "ImageSetRegistry의 imageSets 리스트 크기를 확인하세요.", this);

        return result;
    }

    /// <summary>현재 할당된 Globe Index 반환</summary>
    public int GlobeIndex => _globeIndex;

}
