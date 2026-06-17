using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using Cysharp.Threading.Tasks;
using Oculus.Skinning;

public class SkinRenderBinder : MonoBehaviour
{
    [SerializeField] private VisualEffect targetVFX;
    [SerializeField] private string vfxParameterName = "Mesh Dissolve";

    private void OnEnable()
    {
       // OvrAvatarUnitySkinnedRenderable.OnSkinnedRendererReady += AssignToVFX;
    }

    private void OnDisable()
    {
    //   OvrAvatarUnitySkinnedRenderable.OnSkinnedRendererReady -= AssignToVFX;
    }

    private void AssignToVFX(SkinnedMeshRenderer skinnedMesh)
    {
        if (targetVFX == null) return;

        if (targetVFX.HasSkinnedMeshRenderer(vfxParameterName))
            targetVFX.SetSkinnedMeshRenderer(vfxParameterName, skinnedMesh);
    }
}
