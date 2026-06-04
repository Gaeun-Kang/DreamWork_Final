using System.Collections.Generic;
using DG.Tweening;
using INab.Dissolve;
using UnityEngine.VFX;
using UnityEngine;
using UnityEngine.Playables;

public class AvatarDissolver : MonoBehaviour
{
    [SerializeField] private GameObject m_Prefab;
    [SerializeField] private Dissolver m_Dissolver;
    [SerializeField] private PlayableDirector m_Director;
    //morph VFX를 제어하도록 변경 

    [Header("Tween Settings")]
    [SerializeField] private float m_TweenDuration = 2f;
    [SerializeField] private Ease m_TweenEase = Ease.Linear;
    [SerializeField] private float m_StartValue = -0.4f;
    [SerializeField] private float m_EndValue = 1f;
    
    public void Play(List<Renderer> renderers)
    {
        m_Dissolver.materials.Clear();
        foreach(Renderer rend in renderers)
        {
            m_Dissolver.materials.AddRange(rend.sharedMaterials);
        }
        
        m_Dissolver.MaterialsDissolveValue = m_StartValue;

        if(m_Dissolver.MaterialsDissolveValue > 0) m_Director.DOPlay();
        Debug.Log("m_Director 시작");


        DOTween.To(
            () => m_Dissolver.MaterialsDissolveValue,
            x => m_Dissolver.MaterialsDissolveValue = x,
            m_EndValue,
            m_TweenDuration
        ).SetEase(m_TweenEase);
    }
}
