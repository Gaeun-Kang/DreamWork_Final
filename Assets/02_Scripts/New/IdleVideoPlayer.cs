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
        if (videoPlayer != null) videoPlayer.Play();
    }

    private void ResetTimerAndHideVideo()
    {
        _idleTimer = 0f;

        if (_isVideoActive)
        {
            _isVideoActive = false;

            if (videoPlayer != null)
            {
                // 1. 영상을 일시정지하고
                videoPlayer.Pause();

                // 2. 재생 시점을 강제로 0번째 프레임(처음)으로 되돌립니다.
                videoPlayer.frame = 0;

                // 3. 완전히 정지 상태로 만듭니다.
                videoPlayer.Stop();
            }

            if (videoUIObject != null)
            {
                videoUIObject.SetActive(false); // UI를 완전히 숨김
            }
        }
    }
  }