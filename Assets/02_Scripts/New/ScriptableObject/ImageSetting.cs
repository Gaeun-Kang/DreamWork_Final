using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "ImageSetting", menuName = "")]
public class ImageSetting : ScriptableObject
{

    [SerializeField]
    private List<ImageSetData> imageSets = new List<ImageSetData>();
    private Dictionary<string, ImageSetData> _ImageSetDataDic;

    public void Initialize()
    {
        _ImageSetDataDic = new Dictionary<string, ImageSetData>();
        foreach (var set in imageSets)
        {
            if (set != null && !string.IsNullOrEmpty(set.setID))
                _ImageSetDataDic[set.setID] = set;
        }
    }

    public ImageSetData GetBySetID(string setID)
    {
        if(_ImageSetDataDic == null) Initialize();
        _ImageSetDataDic.TryGetValue(setID, out var result);
        return result; 
    }

    public ImageSetData GetByMainTexture(Texture2D mainTexture)
    {
        foreach (var set in imageSets)
        {
            if (set != null && set.mainImage == mainTexture)
                return set;
        }
        return null;
    }

}
