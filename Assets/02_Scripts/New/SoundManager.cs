using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 이벤트별 BGM을 관리하는 싱글톤 SoundManager.
/// BGM 페이드 인/아웃, SFX 재생, 볼륨 제어를 지원합니다.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public enum GameEvent
    {
        Main_1,
        Main_2,

        UI_Dream_hover,
        UI_Dream_click,
        UI_Menu_click,

        Dream_Happy,
        Dream_Nervo,
        Dream_Sad,

        World_Trans
    }

    [System.Serializable]
    public class BGMEntry
    {
        public GameEvent gameEvent;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = true;
    }


    [Header("BGM 설정")]
    [SerializeField] private List<BGMEntry> bgmEntries = new();
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("SFX 설정")]
    [SerializeField] private List<AudioClip> _sfxClip = new();
    [SerializeField] private int sfxPoolSize = 8;

    [Header("초기 볼륨")]
    [Range(0f, 1f)][SerializeField] private float bgmMasterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxMasterVolume = 1f;


    private AudioSource _bgmSource;
    private AudioSource _bgmSourceB;          // 크로스페이드용 보조 소스
    private bool _isSourceAActive = true;

    private List<AudioSource> _sfxPool = new();
    private Dictionary<GameEvent, BGMEntry> _bgmMap = new();

    private GameEvent _currentEvent;
    private Coroutine _fadeCoroutine;


    public float BGMMasterVolume
    {
        get => bgmMasterVolume;
        set
        {
            bgmMasterVolume = Mathf.Clamp01(value);
            ApplyBGMVolume();
        }
    }

    public float SFXMasterVolume
    {
        get => sfxMasterVolume;
        set => sfxMasterVolume = Mathf.Clamp01(value);
    }

    public GameEvent CurrentEvent => _currentEvent;
    public bool IsBGMPlaying => ActiveBGMSource.isPlaying;

    private AudioSource ActiveBGMSource => _isSourceAActive ? _bgmSource : _bgmSourceB;
    private AudioSource InactiveBGMSource => _isSourceAActive ? _bgmSourceB : _bgmSource;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();
        BuildBGMMap();

    
    }

    private void InitAudioSources()
    {
        _bgmSource = CreateAudioSource("BGM_A");
        _bgmSourceB = CreateAudioSource("BGM_B");

        for (int i = 0; i < sfxPoolSize; i++)
            _sfxPool.Add(CreateAudioSource($"SFX_{i}"));
    }

    private AudioSource CreateAudioSource(string sourceName)
    {
        var go = new GameObject(sourceName);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }

    private void BuildBGMMap()
    {
        _bgmMap.Clear();
        foreach (var entry in bgmEntries)
        {
            if (!_bgmMap.ContainsKey(entry.gameEvent))
                _bgmMap[entry.gameEvent] = entry;
            else
                Debug.LogWarning($"[SoundManager] 중복된 GameEvent: {entry.gameEvent}. 첫 번째 항목만 사용됩니다.");
        }
    }


    public void PlayBGM(GameEvent gameEvent, bool crossFade = true)
    {
        if (_currentEvent == gameEvent && ActiveBGMSource.isPlaying) return;

        if (!_bgmMap.TryGetValue(gameEvent, out var entry))
        {
            Debug.LogWarning($"[SoundManager] BGMEntry 없음: {gameEvent}");
            return;
        }

        _currentEvent = gameEvent;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = crossFade
            ? StartCoroutine(CrossFade(entry))
            : StartCoroutine(FadeInNew(entry));
    }

    /// <summary>현재 BGM을 페이드 아웃 후 정지합니다.</summary>
    public void StopBGM(bool fade = true)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = fade
            ? StartCoroutine(FadeOut(ActiveBGMSource, fadeDuration))
            : null;

        if (!fade)
        {
            ActiveBGMSource.Stop();
            InactiveBGMSource.Stop();
        }
    }

    /// <summary>BGM을 일시정지합니다.</summary>
    public void PauseBGM() => ActiveBGMSource.Pause();

    /// <summary>BGM을 재개합니다.</summary>
    public void ResumeBGM() => ActiveBGMSource.UnPause();


    /// <summary>SFX 클립을 풀에서 재생합니다.</summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        var src = GetAvailableSFXSource();
        if (src == null)
        {
            Debug.LogWarning("[SoundManager] SFX 풀이 가득 찼습니다.");
            return;
        }

        src.clip = clip;
        src.volume = volume * sfxMasterVolume;
        src.pitch = pitch;
        src.loop = false;
        src.Play();
    }

    public void PlaySFXByIndex(int clipIndex, float volume = 1f, float pitch = 1f)
    {
        if (_sfxClip == null || clipIndex < 0 || clipIndex >= _sfxClip.Count)
        {
            Debug.LogWarning($"[SoundManager] 유효하지 않은 SFX 인덱스 번호입니다: {clipIndex}");
            return;
        }

        PlaySFX(_sfxClip[clipIndex], volume, pitch);
    }


    /// <summary>SFX를 월드 좌표에서 재생합니다 (3D 사운드).</summary>
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume * sfxMasterVolume);
    }
    private IEnumerator CrossFade(BGMEntry entry)
    {
        var outSource = ActiveBGMSource;
        var inSource = InactiveBGMSource;

        // 새 소스 준비
        inSource.clip = entry.clip;
        inSource.loop = entry.loop;
        inSource.volume = 0f;
        inSource.Play();

        float elapsed = 0f;
        float startVol = outSource.volume;
        float targetVol = entry.volume * bgmMasterVolume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            outSource.volume = Mathf.Lerp(startVol, 0f, t);
            inSource.volume = Mathf.Lerp(0f, targetVol, t);
            yield return null;
        }

        outSource.Stop();
        outSource.volume = 0f;
        inSource.volume = targetVol;

        _isSourceAActive = !_isSourceAActive;   // 소스 역할 교체
    }

    private IEnumerator FadeInNew(BGMEntry entry)
    {
        // 기존 소스 즉시 정지
        ActiveBGMSource.Stop();
        InactiveBGMSource.Stop();

        var src = ActiveBGMSource;
        src.clip = entry.clip;
        src.loop = entry.loop;
        src.volume = 0f;
        src.Play();

        float elapsed = 0f;
        float targetVol = entry.volume * bgmMasterVolume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, targetVol, elapsed / fadeDuration);
            yield return null;
        }

        src.volume = targetVol;
    }

    private IEnumerator FadeOut(AudioSource src, float duration)
    {
        float startVol = src.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }

        src.Stop();
        src.volume = 0f;
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var src in _sfxPool)
            if (!src.isPlaying) return src;
        return null;
    }

    private void ApplyBGMVolume()
    {
        if (_bgmMap.TryGetValue(_currentEvent, out var entry))
            ActiveBGMSource.volume = entry.volume * bgmMasterVolume;
    }

#if UNITY_EDITOR
    [ContextMenu("테스트: MainMenu BGM 재생")]
    private void TestMainMenu() => PlayBGM(GameEvent.Main_1);

    [ContextMenu("테스트: BGM 정지")]
    private void TestStop() => StopBGM();
#endif

}