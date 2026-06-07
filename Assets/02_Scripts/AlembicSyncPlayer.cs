using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;

public class AlembicSyncPlayer : MonoBehaviour
{
    [Header("Alembic Players")]
    public AlembicStreamPlayer[] players;

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
        foreach (var p in players)
        {
            if (p == null) continue;
            p.CurrentTime = time;
            p.UpdateImmediately(time);
        }
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