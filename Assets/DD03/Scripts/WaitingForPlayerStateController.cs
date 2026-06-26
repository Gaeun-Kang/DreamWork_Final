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

    // 원래 아바타의 위치와 스케일을 기억해두기 위한 변수 (초기화용)
    private Vector3 m_OriginalAvatarLocalPos;
    private Vector3 m_OriginalAvatarLocalScale;

    private void Awake()
    {
        GameFlowManager.OnGameStateChanged += HandleGameStateChanged;
        OVRManager.HMDMounted += OnHMDMounted;
        OVRManager.HMDUnmounted += OnHMDUnmounted;

        // 아바타 초기 위치 및 스케일 백업
        if (m_LocalAvatar != null)
        {
            m_OriginalAvatarLocalPos = m_LocalAvatar.transform.localPosition;
            m_OriginalAvatarLocalScale = m_LocalAvatar.transform.localScale;
        }
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Main_1);

        // [핵심] 씬이 새로 로드될 때 무조건 상태를 완전히 리셋합니다.
        ForceResetAndEvaluate();
    }

    private void OnDestroy()
    {
        GameFlowManager.OnGameStateChanged -= HandleGameStateChanged;
        OVRManager.HMDMounted -= OnHMDMounted;
        OVRManager.HMDUnmounted -= OnHMDUnmounted;

        // 씬이 나갈 때 작동 중인 DOTween과 비동기(UniTask) 완전 제거
        CancelCurrentFlow();
        DOTween.Kill(m_LocalAvatar.transform);
    }

    /// <summary>
    /// 씬 진입 시 기존 상태를 무시하고 강제로 완전히 초기화한 후 흐름을 평가하는 함수
    /// </summary>
    private void ForceResetAndEvaluate()
    {
        // 1. 기존 비동기 태스크 및 UI 강제 리셋
        ResetFlow();

        // 2. DOTween 찌꺼기 제거 및 아바타 위치/스케일/카메라 뷰 원상복구
        DOTween.Kill(m_LocalAvatar.transform);
        if (m_LocalAvatar != null)
        {
            m_LocalAvatar.transform.localPosition = m_OriginalAvatarLocalPos;
            m_LocalAvatar.transform.localScale = m_OriginalAvatarLocalScale;

            // 아바타 뷰를 1인칭(FirstPerson)으로 초기화
            var config = m_LocalAvatar.GetAvatarConfig();
            config.ActiveView = CAPI.ovrAvatar2EntityViewFlags.FirstPerson;
            m_LocalAvatar.ApplyConfig(config, requiresTeardown: false);
        }

        // 3. HMD 착용 상태 물리적으로 즉시 체크 (이벤트에만 의존하지 않음)
        m_IsHmdMounted = OVRManager.isHmdPresent;

        // 4. [중요] 만약 GameFlowManager가 DDOL이라 상태가 꼬여있다면 강제로 WaitingForPlayer로 재설정
        if (GameFlowManager.Instance != null)
        {
            if (GameFlowManager.Instance.GameState != GameState.WaitingForPlayer)
            {
                GameFlowManager.Instance.ChangeState(GameState.WaitingForPlayer);
            }
            else
            {
                // 이미 상태가 WaitingForPlayer라면 이벤트가 안 바뀌므로 코드를 직접 강제 구동시킵니다.
                if (m_IsHmdMounted)
                {
                    StartFlow();
                }
            }
        }
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
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.GameState == GameState.WaitingForPlayer)
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

        if (m_Section2UI != null) m_Section2UI.SetActive(false);
        if (m_Section4UI != null) m_Section4UI.SetActive(false);
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

            // Step 2: Wait until hands detected
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

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.ChangeState(GameState.Dissolve);
            }
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

        if (localPos.z < 0 || localPos.z > 0.8f) return false;
        if (Mathf.Abs(localPos.x) > 0.3f) return false;
        if (localPos.y < -0.3f || localPos.y > 0.3f) return false;

        return true;
    }

    private async UniTask AnimateAvatarForward(CancellationToken token)
    {
        if (m_LocalAvatar == null) return;

        Transform avatar = m_LocalAvatar.transform;
        m_LocalAvatar.Hidden = false;
        Vector3 startPos = avatar.localPosition;
        Vector3 targetPos = startPos + Vector3.forward * m_MoveDistance;

        // 1초 후 뷰 전환 예약 (token 연결을 통해 도중에 취소되면 실행 안 되게 방어)
        DOVirtual.DelayedCall(1f, SwitchToThirdPerson).SetLink(gameObject);

        // 앞으로 이동
        Tween moveTween = avatar.DOLocalMove(targetPos, m_MoveDuration)
            .SetEase(m_ForwardCurve);

        // 이동이 끝날 때까지 대기
        await moveTween.AsyncWaitForCompletion().AsUniTask();

        // 스케일 반전 전 토큰 확인
        if (token.IsCancellationRequested) return;

        // scale.z 1 → -1 애니메이션
        Vector3 scale = avatar.localScale;
        Tween flipTween = avatar.DOScale(new Vector3(scale.x, scale.y, -Mathf.Abs(scale.z)), 1f)
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