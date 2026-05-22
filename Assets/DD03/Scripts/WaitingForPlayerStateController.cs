using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Oculus.Avatar2;
using UnityEngine;

public class WaitingForPlayerStateController : MonoBehaviour
{
    [Header("Avatar Split Animation")]
    [SerializeField] private SampleAvatarEntity m_LocalAvatar;
    [SerializeField] private AnimationCurve m_ForwardCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float m_MoveDuration = 3f;
    [SerializeField] private float m_MoveDistance = 0.5f;
    
    [Header("UI References")]
    [SerializeField] private GameObject m_Section2UI; // "손을 얼굴 앞으로 올려 보세요"
    [SerializeField] private GameObject m_Section4UI; // "당신의 무의식이 분리되고 있어요"
    [SerializeField] private GameObject m_GhostAvatarHands; // Meta avatar hands

    private bool m_IsHmdMounted;
    private CancellationTokenSource m_Cts;

    private void Awake()
    {
        GameFlowManager.OnGameStateChanged += HandleGameStateChanged;
        OVRManager.HMDMounted += OnHMDMounted;
        OVRManager.HMDUnmounted += OnHMDUnmounted;
    }

    private void OnDestroy()
    {
        GameFlowManager.OnGameStateChanged -= HandleGameStateChanged;
        OVRManager.HMDMounted -= OnHMDMounted;
        OVRManager.HMDUnmounted -= OnHMDUnmounted;
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.WaitingForPlayer && m_IsHmdMounted)
            StartFlow();
        else
            ResetFlow();
    }

    private void OnHMDMounted()
    {
        m_IsHmdMounted = true;
        if (GameFlowManager.Instance.GameState == GameState.WaitingForPlayer)
            StartFlow();
    }

    private void OnHMDUnmounted()
    {
        m_IsHmdMounted = false;
        ResetFlow();
    }

    private void StartFlow()
    {
        CancelCurrentFlow();
        m_Cts = new CancellationTokenSource();
        RunFlowAsync(m_Cts.Token).Forget();
    }

    private void ResetFlow()
    {
        CancelCurrentFlow();

        m_Section2UI.SetActive(false);
        m_Section4UI.SetActive(false);
        //m_GhostAvatarHands.SetActive(false);
    }

    private void CancelCurrentFlow()
    {
        if (m_Cts != null)
        {
            m_Cts.Cancel();
            m_Cts.Dispose();
            m_Cts = null;
        }
    }

    private async UniTaskVoid RunFlowAsync(CancellationToken token)
    {
        try
        {
            // Step 1: After 5 sec → Section 2 UI
            await UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token);
            m_Section2UI.SetActive(true);

            // Step 2: Wait until hands detected (replace with your hand tracking condition)
            await UniTask.WaitUntil(() => HandsAreVisible(), cancellationToken: token);
            m_Section2UI.SetActive(false);

            // Step 3: After 4 sec → Section 4 UI
            await UniTask.Delay(TimeSpan.FromSeconds(4), cancellationToken: token);
            m_Section4UI.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);
            m_Section4UI.SetActive(false);
            
            // Step 4: Animate local avatar moving forward
            await AnimateAvatarForward(token);

            await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token);
            
            GameFlowManager.Instance.ChangeState(GameState.Dissolve);
        }
        catch (OperationCanceledException)
        {
            // Flow interrupted (HMD off or state changed)
        }
    }

    private bool HandsAreVisible()
    {
        var rig = PlayerRigRef.Instance;
        if (rig == null) return false;

        return IsHandInFront(rig.LeftHand, rig.CenterEyeAnchor) || 
               IsHandInFront(rig.RightHand, rig.CenterEyeAnchor);
    }

    private bool IsHandInFront(OVRHand hand, Transform hmdCenter)
    {
        if (hand == null || !hand.IsTracked || hand.HandConfidence != OVRHand.TrackingConfidence.High)
            return false;

        Vector3 localPos = hmdCenter.InverseTransformPoint(hand.transform.position);

        // Example thresholds
        if (localPos.z < 0 || localPos.z > 0.8f) return false;
        if (Mathf.Abs(localPos.x) > 0.3f) return false;
        if (localPos.y < -0.3f || localPos.y > 0.3f) return false;

        return true;
    }
    
    private async UniTask AnimateAvatarForward(CancellationToken token)
    {
        Transform avatar = m_LocalAvatar.transform;
        m_LocalAvatar.Hidden = false;
        Vector3 startPos = avatar.localPosition;
        Vector3 targetPos = startPos + Vector3.forward * m_MoveDistance;

        // 1초 후 뷰 전환 예약
        DOVirtual.DelayedCall(1f, SwitchToThirdPerson);

        // 앞으로 이동
        Tween moveTween = avatar.DOLocalMove(targetPos, m_MoveDuration)
            .SetEase(m_ForwardCurve);

        // 이동이 끝날 때까지 대기
        await moveTween.AsyncWaitForCompletion().AsUniTask();

        // scale.z 1 → -1 애니메이션
        Vector3 scale = avatar.localScale;
        Tween flipTween = avatar.DOScale(new Vector3(scale.x, scale.y, -Mathf.Abs(scale.z)), 1f) // 1초 동안
            .SetEase(Ease.InOutSine);

        await flipTween.AsyncWaitForCompletion().AsUniTask();
    }

    private void SwitchToThirdPerson()
    {
        if (m_LocalAvatar == null) return;

        var config = m_LocalAvatar.GetAvatarConfig();
        config.ActiveView = CAPI.ovrAvatar2EntityViewFlags.ThirdPerson;
        m_LocalAvatar.ApplyConfig(config, requiresTeardown: false);
    }
}
