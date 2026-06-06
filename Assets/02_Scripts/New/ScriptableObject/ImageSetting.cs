using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImageSetting", menuName = "Scriptable Objects/Image Setting")]
public class ImageSetting : ScriptableObject
{

    [SerializeField]
    private List<ImageSetData> imageSets = new List<ImageSetData>();

    private Dictionary<string, ImageSetData> _idLookup;
    private Dictionary<Texture2D, ImageSetData> _textureLookup;

    public void Initialize()
    {
        _idLookup = new Dictionary<string, ImageSetData>();
        _textureLookup = new Dictionary<Texture2D, ImageSetData>();

        foreach (var set in imageSets)
        {
            if (set == null) continue;

            if (!string.IsNullOrEmpty(set.setID) && !_idLookup.ContainsKey(set.setID))
                _idLookup[set.setID] = set;

            if (set.mainImage != null && !_textureLookup.ContainsKey(set.mainImage))
                _textureLookup[set.mainImage] = set;
        }
    }

    // globeTextures 배열 인덱스로 ImageSetData를 직접 조회합니다.
    // imageSets 리스트 순서를 globeTextures와 동일하게 배치 
    public ImageSetData GetByIndex(int index)
    {
        if (imageSets == null || imageSets.Count == 0)
        {
            Debug.LogWarning("[ImageSetRegistry] imageSets가 비어있습니다.");
            return null;
        }

        int clamped = index % imageSets.Count;
        var result = imageSets[clamped];

        if (result == null)
            Debug.LogWarning($"[ImageSetRegistry] index [{index}] 의 ImageSetData가 null입니다.");

        return result;
    }

    public int Count => imageSets?.Count ?? 0;


    public ImageSetData GetBySetID(string setID)
    {
        if (_idLookup == null) Initialize();
        _idLookup.TryGetValue(setID, out var result);
        return result;
    }

    //<summary>Main Image 텍스쳐로 ImageSetData 조회 (텍스쳐를 직접 비교할 수 있는 경우)
    public ImageSetData GetByMainTexture(Texture2D mainTexture)
    {
        if (_textureLookup == null) Initialize();
        if (mainTexture == null) return null;
        _textureLookup.TryGetValue(mainTexture, out var result);
        return result;
    }

    public IReadOnlyList<ImageSetData> GetAll() => imageSets.AsReadOnly();
}
