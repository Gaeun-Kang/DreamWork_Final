using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VideoPlayer))]
public class IdleVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float idleThreshold = 60f; // 1분 (초 단위)

    private float _idleTimer = 0f;
    private bool _isVideoPlaying = false;

    void OnEnable()
    {
        // 어떤 입력 액션이든 발생하면 타이머를 리셋하는 이벤트 연결
        InputSystem.onActionChange += OnActionChange;
    }

    void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    void Update()
    {
        // 비디오가 이미 재생 중이라면 타이머를 더 이상 누적하지 않음
        if (_isVideoPlaying) return;

        _idleTimer += Time.deltaTime;

        if (_idleTimer >= idleThreshold)
        {
            PlayIdleVideo();
        }
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        // 사용자가 버튼을 누르거나 컨트롤러/HMD를 움직이는 등 '수행(Performed)' 상태일 때
        if (change == InputActionChange.ActionPerformed)
        {
            ResetIdleTimer();
        }
    }

    private void PlayIdleVideo()
    {
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            _isVideoPlaying = true;
            videoPlayer.Play();
            Debug.Log("사용자 입력 없음: 대기 비디오 재생");
        }
    }

    private void ResetIdleTimer()
    {
        _idleTimer = 0f;

        // 비디오가 재생 중이었다면 입력을 감지한 순간 정지/숨김
        if (_isVideoPlaying)
        {
            _isVideoPlaying = false;
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
                Debug.Log("사용자 입력 감지: 비디오 정지");
            }
        }
    }
}