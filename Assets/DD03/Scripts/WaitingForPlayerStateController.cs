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

    private Vector3 m_OriginalAvatarLocalPos;
    private Vector3 m_OriginalAvatarLocalScale;
    private Vector3 m_DetectedHandWorldPos;

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
        m_LocalAvatar.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightHandWrist);

        //씬 로드시 리셋 
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

        if (IsHandInFront(rig.LeftHand, rig.CenterEyeAnchor))
        {
            m_DetectedHandWorldPos = rig.LeftHand.transform.position;
            return true;
        }

        if (IsHandInFront(rig.RightHand, rig.CenterEyeAnchor))
        {
            m_DetectedHandWorldPos = rig.RightHand.transform.position;
            return true;
        }

        return false;
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

    private float GetAvatarPivotToHandOffset()
    {
        if (m_LocalAvatar == null) return 0f;

        Transform handBone = m_LocalAvatar.GetSkeletonTransform(CAPI.ovrAvatar2JointType.RightHandWrist);
        if (handBone == null) return 0f;

        return handBone.position.y - m_LocalAvatar.transform.position.y;
    }

    private async UniTask AnimateAvatarForward(CancellationToken token)
    {
        if (m_LocalAvatar == null) return;

        Transform avatar = m_LocalAvatar.transform;
        m_LocalAvatar.Hidden = false;

        // 임시 시작 위치 (아바타 현재 로컬 위치 그대로 - 머리 렌더링 중에는 위치가 중요하지 않음)
        Vector3 provisionalStartPos = avatar.localPosition;
        Vector3 provisionalTargetPos = provisionalStartPos + Vector3.forward * m_MoveDistance;

        DOVirtual.DelayedCall(1f, SwitchToThirdPerson).SetLink(gameObject);

        // 1단계: 일단 이동 시작 (아직 손 위치 보정 전)
        Tween moveTween = avatar.DOLocalMove(provisionalTargetPos, m_MoveDuration)
            .SetEase(m_ForwardCurve);

        // 0.3초 대기 (이동은 위 트윈이 계속 진행 중)
        await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);

        if (token.IsCancellationRequested)
        {
            moveTween.Kill();
            return;
        }

        // 2단계: 0.3초 시점에서 진행 중인 트윈을 멈추고, 손 위치로 스냅 후 나머지 이동 재시작
        moveTween.Kill();

        Vector3 currentHandWorldPos = GetCurrentHandWorldPos();
        Vector3 snappedStartPos = avatar.parent != null
            ? avatar.parent.InverseTransformPoint(currentHandWorldPos)
            : currentHandWorldPos;

        float pivotToHandOffset = GetAvatarPivotToHandOffset();
        snappedStartPos.y -= pivotToHandOffset;

        avatar.localPosition = snappedStartPos; // 손 위치로 스냅 (여기서 "분리" 효과)

        Vector3 finalTargetPos = snappedStartPos + Vector3.forward * m_MoveDistance;

        // 남은 거리만큼 이동 시간 비례 계산 (전체 시간에서 이미 흐른 0.3초 제외)
        float remainingDuration = Mathf.Max(m_MoveDuration - 1.0f, 0.01f);

        Tween remainingMoveTween = avatar.DOLocalMove(finalTargetPos, remainingDuration)
            .SetEase(m_ForwardCurve);

        await remainingMoveTween.AsyncWaitForCompletion().AsUniTask();

        if (token.IsCancellationRequested) return;

        Vector3 scale = avatar.localScale;
        Tween flipTween = avatar.DOScale(new Vector3(scale.x, scale.y, -Mathf.Abs(scale.z)), 1f)
            .SetEase(Ease.InOutSine);

        await flipTween.AsyncWaitForCompletion().AsUniTask();
        //GameFlowManager.Instance.ChangeState(GameState.Dream);
    }

    private Vector3 GetCurrentHandWorldPos()
    {
        var rig = PlayerRigRef.Instance;
        if (rig == null) return m_DetectedHandWorldPos; // fallback

        Vector3? handPos = null;

        if (rig.RightHand != null && rig.RightHand.IsTracked)
            handPos = rig.RightHand.transform.position;
        else if (rig.LeftHand != null && rig.LeftHand.IsTracked)
            handPos = rig.LeftHand.transform.position;

        if (handPos == null)
        {
            GameFlowManager.Instance.ChangeState(GameState.Dream);
            return m_DetectedHandWorldPos;
        }

        Vector3 hmdForward = rig.CenterEyeAnchor != null ? rig.CenterEyeAnchor.forward : Vector3.forward;
        Vector3 hmdRight = rig.CenterEyeAnchor != null ? rig.CenterEyeAnchor.right : Vector3.right;

        // 수평면 기준으로만 보정 (위아래 기울임 영향 제거)
        hmdForward.y = 0f;
        hmdRight.y = 0f;
        hmdForward = hmdForward.sqrMagnitude > 0.0001f ? hmdForward.normalized : Vector3.forward;
        hmdRight = hmdRight.sqrMagnitude > 0.0001f ? hmdRight.normalized : Vector3.right;

        const float zOffset = 0.25f;
        const float xOffset = 0.16f; // 오른손 기준 왼쪽으로 보정, 필요시 조정

        return handPos.Value + hmdForward * zOffset - hmdRight * xOffset;
    }

    private void SwitchToThirdPerson()
    {
        if (m_LocalAvatar == null) return;

        var config = m_LocalAvatar.GetAvatarConfig();
        config.ActiveView = CAPI.ovrAvatar2EntityViewFlags.ThirdPerson;
        m_LocalAvatar.ApplyConfig(config, requiresTeardown: false);
    }
}