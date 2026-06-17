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

        happyParticle.gameObject.SetActive(false);
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

        if (_currentParticle != null && _currentParticle != target)
            _currentParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        _currentParticle = target;
        target.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        target.Play();

        Debug.Log($"[EmotionParticlePlayer] setID [{imageSet.setID}] → {target.name} 재생");
    }
    private ParticleSystem GetParticleByID(int id)
    {
        if (id >= 1 && id <= 9)
        {
            SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Happy);
            happyParticle.gameObject.SetActive(true);

        }

        if (id >= 10 && id <= 18) 
        {
        SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Sad);

            sadParticle.gameObject.SetActive(true);


        }

        if (id >= 19 && id <= 26) 
        {
         SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Nervo);
            nervousParticle.gameObject.SetActive(true);


        }
        return null;
    }
}
