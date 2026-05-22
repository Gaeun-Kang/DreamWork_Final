using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class SphereParticleFadeController : MonoBehaviour
{
    [Header("Center")]
    public Transform centerTarget;

    [Header("Distance Range")]
    public float innerRadius = 0f;
    public float outerRadius = 5f;

    [Header("Size At Center")]
    public float centerMinSize = 0.08f;
    public float centerMaxSize = 0.18f;

    [Header("Size At Outer Edge")]
    public float outerMinSize = 0.005f;
    public float outerMaxSize = 0.04f;

    [Header("Falloff Shape")]
    [Range(0.1f, 8f)]
    public float falloffPower = 1f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    private void LateUpdate()
    {
        ApplySizeFalloff();
    }

    private void OnValidate()
    {
        innerRadius = Mathf.Max(0f, innerRadius);
        outerRadius = Mathf.Max(innerRadius + 0.001f, outerRadius);

        centerMinSize = Mathf.Max(0f, centerMinSize);
        centerMaxSize = Mathf.Max(centerMinSize, centerMaxSize);

        outerMinSize = Mathf.Max(0f, outerMinSize);
        outerMaxSize = Mathf.Max(outerMinSize, outerMaxSize);

        Init();
    }

    private void Init()
    {
        if (ps == null)
            ps = GetComponent<ParticleSystem>();

        int maxParticles = ps != null ? ps.main.maxParticles : 0;

        if (particles == null || particles.Length < maxParticles)
            particles = new ParticleSystem.Particle[maxParticles];
    }

    private void ApplySizeFalloff()
    {
        if (ps == null || particles == null) return;

        int count = ps.GetParticles(particles);
        Vector3 center = centerTarget != null ? centerTarget.position : transform.position;

        ParticleSystem.MainModule main = ps.main;
        bool isLocal = main.simulationSpace == ParticleSystemSimulationSpace.Local;

        for (int i = 0; i < count; i++)
        {
            Vector3 particleWorldPos = isLocal
                ? transform.TransformPoint(particles[i].position)
                : particles[i].position;

            float distance = Vector3.Distance(particleWorldPos, center);

            float t = Mathf.InverseLerp(innerRadius, outerRadius, distance);
            t = Mathf.Clamp01(t);
            t = Mathf.Pow(t, falloffPower);

            // 중심부 크기 범위 → 외곽부 크기 범위로 보간
            float minSize = Mathf.Lerp(centerMinSize, outerMinSize, t);
            float maxSize = Mathf.Lerp(centerMaxSize, outerMaxSize, t);

            // particle randomSeed 기반이라 매 프레임 크기가 덜컥거리지 않음
            float randomValue = StableRandom01(particles[i].randomSeed);

            particles[i].startSize = Mathf.Lerp(minSize, maxSize, randomValue);
        }

        ps.SetParticles(particles, count);
    }

    private float StableRandom01(uint seed)
    {
        seed ^= seed << 13;
        seed ^= seed >> 17;
        seed ^= seed << 5;

        return (seed & 0x00FFFFFF) / 16777215f;
    }
}