using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DreamTransitionController : MonoBehaviour
{
    [SerializeField] private Transform m_Water;
    [SerializeField] private float m_ZEndValue;
    [SerializeField] private float m_ZMoveDuration;

    [SerializeField] private List<ParticleSystem> m_GlobeParticles;
    [SerializeField] private List<MeshRenderer> m_MeshRenderers;

    private bool m_Played;

    private void Awake()
    {
        foreach (var renderer in m_MeshRenderers)
        {
            foreach (var mat in renderer.materials)
            {
                mat.SetFloat("_Alpha", 0f);
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (m_Played) return;
        m_Played = true;

        // Step 1: Move along Z
        m_Water.DOLocalMoveX(m_ZEndValue, m_ZMoveDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                foreach (ParticleSystem ps in m_GlobeParticles)
                {
                    ps.Play();
                }
                
                // Step 2: Fade materials after Z move finishes
                foreach (var renderer in m_MeshRenderers)
                {
                    foreach (var mat in renderer.materials) // duplicate per instance
                    {
                        DOTween.To(
                            () => mat.GetFloat("_Alpha"),
                            x => mat.SetFloat("_Alpha", x),
                            1f,
                            2f // duration of fade
                        ).SetEase(Ease.InOutSine);
                    }
                }
            });
    }
}