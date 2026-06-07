using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using INab.Dissolve;
using Oculus.Skinning;
using UnityEngine;

public class DissolveStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AvatarDissolver m_AvatarDissolver;
    [SerializeField] private Dissolver m_WallDissolver;
    [SerializeField] private GameObject m_LocalAvatar;

    [Header("Tween Settings")]
    [SerializeField] private float m_MoveDistance = 2f;
    [SerializeField] private float m_MoveDuration = 2f;
    [SerializeField] private float m_WallDissolveDelay = 1.5f;
    [SerializeField] private float m_WallDissolveDuration = 8f;
    [SerializeField] private float m_LightControlDelay = 4f;
    
    [Header("Light Settings")]
    [SerializeField] private MeshRenderer m_OuterWall;
    [SerializeField] private float m_BlinkDuration = 0.08f;       // 깜빡임 속도
    [SerializeField] private float m_FadeOutDuration = 2f;     // 마지막 서서히 꺼짐 시간
    [SerializeField] private Color m_EmissionOnColor = Color.white;

    private Material m_WallMat;

    private void Awake()
    {
        GameFlowManager.OnGameStateChanged += HandleGameStateChanged;
        
        // Instance material 사용
        m_WallMat = m_OuterWall.material;

        // 시작은 emission 꺼진 상태
        m_WallMat.SetColor("_EmissionColor", Color.black);
        m_WallMat.DisableKeyword("_EMISSION");
    }

    private void OnDestroy()
    {
        GameFlowManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.Dissolve)
        {
            StartDissolveSequence().Forget();
        }
    }

    private async UniTaskVoid StartDissolveSequence()
    {
        // Avatar starts moving immediately
        m_LocalAvatar.transform.DOMoveZ(
            m_LocalAvatar.transform.position.z + m_MoveDistance,
            m_MoveDuration
        ).SetEase(Ease.InOutSine);

        m_LocalAvatar.transform.DOMoveY(
0.5f,
m_MoveDuration
).SetEase(Ease.InOutSine);

        // Wait before avatar dissolve starts
        //await UniTask.Delay(System.TimeSpan.FromSeconds(m_AvatarDissolveDelay));

        // Avatar dissolve begins
        var ovrAvatarSkinnedRenderables = m_LocalAvatar.GetComponentsInChildren<OvrAvatarUnitySkinnedRenderable>().ToList();
        List<Renderer> renderers = new List<Renderer>();
        foreach (var ovrAvatarSkinnedRenderable in ovrAvatarSkinnedRenderables)
        {
            Renderer rend = ovrAvatarSkinnedRenderable.GetComponent<Renderer>();
            if (rend != null)
                renderers.Add(rend);
        }

        m_AvatarDissolver.Play(renderers);
        
        //딜레이가 없는 편이 의도와 맞음
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_WallDissolveDelay));

        // Start wall dissolve AFTER avatar dissolve begins
        m_WallDissolver.MaterialsDissolveValue = 1.5f;
        DOTween.To(
            () => m_WallDissolver.MaterialsDissolveValue,
            x => m_WallDissolver.MaterialsDissolveValue = x,
            0.45f,
            m_WallDissolveDuration
        ).SetEase(Ease.Linear);
        
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_LightControlDelay));

        ControlLight();
    }

    private void ControlLight()
    {
        // 시퀀스 만들기
        Sequence seq = DOTween.Sequence();

        // emission 활성화
        m_WallMat.EnableKeyword("_EMISSION");

        // Blink 2회 (On → Off → On → Off)
        for (int i = 0; i < 2; i++)
        {
            seq.Append(m_WallMat.DOColor(m_EmissionOnColor, "_EmissionColor", m_BlinkDuration));
            seq.Append(m_WallMat.DOColor(Color.black, "_EmissionColor", m_BlinkDuration));
        }
        seq.Append(m_WallMat.DOColor(m_EmissionOnColor, "_EmissionColor", m_BlinkDuration));
        seq.AppendInterval(2);

        // 마지막 Fade Out (서서히 꺼짐)
        seq.Append(m_WallMat.DOColor(Color.black, "_EmissionColor", m_FadeOutDuration));

        // 끝나면 emission 끄기
        seq.OnComplete(() =>
        {
            m_WallMat.DisableKeyword("_EMISSION");
        });

    }
}
