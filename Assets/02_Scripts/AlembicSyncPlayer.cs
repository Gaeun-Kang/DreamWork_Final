using UnityEngine;

// 1. Alembic 패키지가 프로젝트에 있을 때만 아래 네임스페이스를 활성화합니다.
#if UNITY_ENABLE_ALEMBIC || UNITY_EDITOR
using UnityEngine.Formats.Alembic.Importer;
#endif

public class AlembicSyncPlayer : MonoBehaviour
{
    // 2. 변수 선언부도 패키지가 있을 때만 존재하도록 처리합니다.
#if UNITY_ENABLE_ALEMBIC || UNITY_EDITOR
    [Header("Alembic Players")]
    public AlembicStreamPlayer[] players;
#endif

    [Header("Time Range")]
    public float startTime = 0.03333f;
    public float endTime = 20f;

    [Header("Playback")]
    public float playbackSpeed = 1f;
    public bool playOnStart = true;
    public bool loop = true;

    private float currentTime;
    private bool isPlaying;

    void Start()
    {
        currentTime = startTime;
        isPlaying = playOnStart;
        SetTime(currentTime);
    }

    void Update()
    {
        if (!isPlaying) return;

        currentTime += Time.deltaTime * playbackSpeed;

        if (currentTime >= endTime)
        {
            if (loop)
                currentTime = startTime;
            else
            {
                currentTime = endTime;
                isPlaying = false;
            }
        }

        SetTime(currentTime);
    }

    private void SetTime(float time)
    {
        // 3. 기능 실행부도 빌드 시 에러가 나지 않도록 전처리기로 감쌉니다.
#if UNITY_ENABLE_ALEMBIC || UNITY_EDITOR
        foreach (var p in players)
        {
            if (p == null) continue;
            p.CurrentTime = time;
            p.UpdateImmediately(time);
        }
#endif
    }

    public void Restart()
    {
        currentTime = startTime;
        isPlaying = true;
        SetTime(currentTime);
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void Play()
    {
        isPlaying = true;
    }
}