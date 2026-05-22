using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleDensityByGroundMask : MonoBehaviour
{
    [Header("References")]
    public Transform groundCenter;
    public LayerMask groundLayer;

    [Header("Spawn Area")]
    public float maxRadius = 8f;
    public float raycastHeight = 10f;
    public float raycastDistance = 30f;

    [Header("Same values as particle shader")]
    public float groundInnerRadius = 2f;
    public float groundOuterRadius = 10f;
    public float groundFadePower = 1f;

    [Header("Density Control")]
    public int maxAttempts = 40;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private HashSet<uint> initializedSeeds = new HashSet<uint>();

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void LateUpdate()
    {
        int count = ps.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            uint seed = particles[i].randomSeed;

            // 이미 배치한 파티클은 건드리지 않음
            if (initializedSeeds.Contains(seed))
                continue;

            if (TryGetDensityPosition(out Vector3 worldPos))
            {
                if (ps.main.simulationSpace == ParticleSystemSimulationSpace.Local)
                    particles[i].position = transform.InverseTransformPoint(worldPos);
                else
                    particles[i].position = worldPos;
            }

            initializedSeeds.Add(seed);
        }

        ps.SetParticles(particles, count);

        // 너무 커지는 것 방지
        if (initializedSeeds.Count > ps.main.maxParticles * 2)
            initializedSeeds.Clear();
    }

    bool TryGetDensityPosition(out Vector3 result)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * maxRadius;

            Vector3 rayStart = groundCenter.position 
                + new Vector3(randomCircle.x, raycastHeight, randomCircle.y);

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
                continue;

            float dist = Vector2.Distance(
                new Vector2(hit.point.x, hit.point.z),
                new Vector2(groundCenter.position.x, groundCenter.position.z)
            );

            float fade = Mathf.Clamp01(
                (dist - groundInnerRadius) / Mathf.Max(0.0001f, groundOuterRadius - groundInnerRadius)
            );

            float groundMask = 1f - fade;
            groundMask = Mathf.Pow(groundMask, groundFadePower);

            // groundMask가 높을수록 채택 확률 증가
            if (Random.value <= groundMask)
            {
                result = hit.point;
                return true;
            }
        }

        result = groundCenter.position;
        return false;
    }
}