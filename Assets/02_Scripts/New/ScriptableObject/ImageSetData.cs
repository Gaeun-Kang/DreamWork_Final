using UnityEngine;

[CreateAssetMenu(fileName = "ImageSetData", menuName = "Scriptable Objects/ImageSetData")]
public class ImageSetData : ScriptableObject
{
    [Header("Set ID (ex: 01, 02, 03)")]
    public string setID;

    [Header("Images")]
    public Texture2D mainImage; //Dream Sphere Material 
    public Texture2D relateImage; //Portal Material
    public Cubemap realImage; // SkyBox Material 
}
