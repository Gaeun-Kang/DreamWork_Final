using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(VideoPlayer))]
public class IdleVideoPlayer : MonoBehaviour
{
    [Header("UI & Video Settings")]
    [SerializeField] private GameObject videoUIObject; // 전체 화면을 덮고 있는 Raw Image 또는 Canvas 오브젝트
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float idleThreshold = 60f; // 1분

    private float _idleTimer = 0f;
    private bool _isVideoActive = false;

    void Start()
    {
        if (videoUIObject != null) videoUIObject.SetActive(false);
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
    }

    void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
    }

    void OnDisable()
    {
        InputSystem.onActionChange -= OnActionChange;
    }

    void Update()
    {
        if (_isVideoActive) return;

        _idleTimer += Time.deltaTime;

        if (_idleTimer >= idleThreshold)
        {
            ShowVideo();
        }
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionStarted) 
        {
            // 입력이 들어오면 타이머를 리셋하고 영상을 완전히 숨김
            ResetTimerAndHideVideo();
        }
    }

    private void ShowVideo()
    {
        _isVideoActive = true;
        _idleTimer = 0f;

        if (videoUIObject != null) videoUIObject.SetActive(true); // 오브젝트 출현

        if (videoPlayer != null)
        {
            videoPlayer.frame = 0; // 켜지는 순간 무조건 0번 프레임부터 시작하도록 대입
            videoPlayer.Play();
        }
    }

    private void ResetTimerAndHideVideo()
    {
        _idleTimer = 0f;

        if (_isVideoActive)
        {
            _isVideoActive = false;

            if (videoPlayer != null)
            {
                videoPlayer.Stop(); // 여기서는 멈추기만 하고, 프레임 리셋은 켜질 때 담당하도록 분리
            }

            if (videoUIObject != null)
            {
                videoUIObject.SetActive(false);
            }
        }
    }
}