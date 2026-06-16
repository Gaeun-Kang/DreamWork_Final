using UnityEngine;

public class EmotionParticlePlayer : MonoBehaviour 
{
    [Header("Emotion Particle System")]
    [Tooltip("01~09번 세트에 대응하는 Happy 파티클")]
    [SerializeField] private ParticleSystem happyParticle;

    [Tooltip("10~18번 세트에 대응하는 Sad 파티클")]
    [SerializeField] private ParticleSystem sadParticle;

    [Tooltip("19~26번 세트에 대응하는 Nervous 파티클")]
    [SerializeField] private ParticleSystem nervousParticle;

    private ParticleSystem _currentParticle;

    public static EmotionParticlePlayer Instance { get; private set; }

 
    public void PlayParticleForSet(ImageSetData imageSet)
    {
        if (imageSet == null)
        {
            Debug.LogWarning("[GlobeParticlePlayer] ImageSetData가 null입니다.");
            return;
        }

        if (!int.TryParse(imageSet.setID, out int id))
        {
            Debug.LogWarning($"[GlobeParticlePlayer] setID '{imageSet.setID}'를 숫자로 변환할 수 없습니다.");
            return;
        }

        ParticleSystem target = GetParticleByID(id);
        if (target == null)
        {
            Debug.LogWarning($"[GlobeParticlePlayer] setID [{id}]에 해당하는 파티클이 없습니다.");
            return;
        }

        // 이전 파티클 정지
        if (_currentParticle != null && _currentParticle != target)
            _currentParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        _currentParticle = target;
        target.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        target.Play();

        Debug.Log($"[GlobeParticlePlayer] Set [{imageSet.setID}] → {target.name} 재생");
    }
    private ParticleSystem GetParticleByID(int id)
    {
        if (id >= 1 && id <= 9)
        {
            SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Happy);
            return happyParticle;
        }

        if (id >= 10 && id <= 18) 
        {
        SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Sad);
        return sadParticle;

        }

        if (id >= 19 && id <= 26) 
        {
         SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Dream_Nervo);
         return nervousParticle;
        } 
        return null;
    }
}
