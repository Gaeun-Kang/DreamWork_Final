using UnityEngine;

[RequireComponent(typeof(ParticleSystemRenderer))]
public class LivingParticleController : MonoBehaviour
{
    public Transform affector;

    private ParticleSystemRenderer psr;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int AffectorID = Shader.PropertyToID("_Affector");

    void Awake()
    {
        psr = GetComponent<ParticleSystemRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        if (affector == null || psr == null) return;

        psr.GetPropertyBlock(propertyBlock);
        propertyBlock.SetVector(AffectorID, new Vector4(
            affector.position.x,
            affector.position.y,
            affector.position.z,
            0f
        ));
        psr.SetPropertyBlock(propertyBlock);
    }
}