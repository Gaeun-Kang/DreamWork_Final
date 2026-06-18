using UnityEngine;

public class EmotionParticlePlayer : MonoBehaviour
{
    [Header("Emotion Particle System")]
    [Tooltip("01~09번 세트에 대응하는 Happy 파티클")]
    [SerializeField] private GameObject happyParticle;

    [Tooltip("10~18번 세트에 대응하는 Sad 파티클")]
    [SerializeField] private GameObject sadParticle;

    [Tooltip("19~26번 세트에 대응하는 Nervous 파티클")]
    [SerializeField] private GameObject nervousParticle;

    private ParticleSystem _currentParticle;

    public static EmotionParticlePlayer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 시작할 때 모든 파티클 오브젝트를 꺼둡니다.
        happyParticle.SetActive(false);
        sadParticle.SetActive(false);
        nervousParticle.SetActive(false);
    }

    public void PlayParticleForSet(ImageSetData imageSet)
    {
        if (imageSet == null)
        {
            Debug.LogWarning("[EmotionParticlePlayer] ImageSetData가 null입니다.");
            return;
        }

        if (!int.TryParse(imageSet.setID, out int id))
        {
            Debug.LogWarning($"[EmotionParticlePlayer] setID '{imageSet.setID}'를 숫자로 변환할 수 없습니다.");
            return;
        }

        ParticleSystem target = GetParticleByID(id);
        if (target == null)
        {
            Debug.LogWarning($"[EmotionParticlePlayer] setID [{id}]에 해당하는 파티클이 없습니다. (범위: 1~26)");
            return;
        }

        // 컴포넌트 자체의 재생/정지 제어
        if (_currentParticle != null && _currentParticle != target)
            _currentParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        _currentParticle = target;
        target.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        target.Play();

        Debug.Log($"[EmotionParticlePlayer] setID [{imageSet.setID}] → {target.name} 재생");
    }

    private ParticleSystem GetParticleByID(int id)
    {
        // 1. 새로운 감정이 정해지기 전에 기존의 모든 오브젝트를 일단 끕니다.
        happyParticle.SetActive(false);
        sadParticle.SetActive(false);
        nervousParticle.SetActive(false);

        // 2. ID 조건에 맞는 오브젝트만 켜고, 해당 ParticleSystem 컴포넌트를 반환합니다.
        if (id >= 1 && id <= 9)
        {
            SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Happy);
            happyParticle.SetActive(true);
            return happyParticle.GetComponent<ParticleSystem>();
        }

        if (id >= 10 && id <= 18)
        {
            SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Sad);
            sadParticle.SetActive(true);
            return sadParticle.GetComponent<ParticleSystem>();
        }

        if (id >= 19 && id <= 26)
        {
            SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Nervo);
            nervousParticle.SetActive(true);
            return nervousParticle.GetComponent<ParticleSystem>();
        }

        return null; // 범위에서 벗어난 ID일 경우 null 반환
    }
}