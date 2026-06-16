using INab.Dissolve;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

public class DissolvingEventManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Dome Mesh")]
    [SerializeField] private MeshRenderer DomeMeshrenderer;

    [Header("Dissolve Setting")]
    [SerializeField] private Material AfterDissolve;
    [SerializeField] private Dissolver mainDissolver;
    [SerializeField] private Volume volume;

   [SerializeField] private AttachDreamGlobesToSpikeTips_Test AttachDream;

    [Header("Morph Setting")]
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private UniformMeshBaker MorphBaker;
    [SerializeField] private GameObject Morphpreafab;
  

  
    private bool Dissolved = false;
    private bool Morphing = false;

    private void Awake()
    {
        if (DomeMeshrenderer == null) Debug.LogError("MeshRenderer 누락");
        playableDirector.time = 0;

        
    }

    void OnEnable()
    {
        mainDissolver.OnDissolve += DissolvingEvent;
    }

    private void OnDisable()
    {
        mainDissolver.OnDissolve -= DissolvingEvent;
    }


    //1. Material 교체 및 사운드 관련 

    private void DissolvingEvent(float value)
    {
         /* DissolveStateController로 이동 
        if (value < 1.25f && Morphing == false)
        {
            playableDirector.time += Time.deltaTime;
            SoundManager.Instance.PlaySFXByIndex(0, volume: 0.8f);
            playableDirector.Evaluate();
            Morphing = true;
        }*/

        if (value < 0.8f && Morphing == true) 
        {
           // MorphBaker.enabled = false;
            SoundManager.Instance.PlaySFXByIndex(1, volume: 0.8f);

        }

        if (value < 0.35f && Dissolved == false) {

           // vignette.active = true; 
            DomeMeshrenderer.material = AfterDissolve;
            SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Main_2);
            AttachDream.AttachDreamGlobes();
            Dissolved = true;

        }
        
    }
}
