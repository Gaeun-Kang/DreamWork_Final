using INab.Dissolve;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

public class DissolvingEventManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Mesh")]
    [SerializeField] private MeshRenderer DomeMeshrenderer;
    [SerializeField] private MeshRenderer Stairrenderer;
    [SerializeField] private MeshRenderer DwonStairrender;



    [Header("Dissolve Setting")]
    [SerializeField] private Material AfterDissolveDome;
    [SerializeField] private Material AfterDissolveStair;
    [SerializeField] private Material AfterDissolveDownStair;
    [SerializeField] private Dissolver mainDissolver;
    [SerializeField] private Volume volume;

   [SerializeField] private AttachDreamGlobesToSpikeTips_Test AttachDream;

  
  
    private bool Dissolved = false;
    private bool Morphing = false;

    private void Awake()
    {
        if (DomeMeshrenderer == null) Debug.LogError("MeshRenderer 누락");

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
            DomeMeshrenderer.material = AfterDissolveDome;
            Stairrenderer.material = AfterDissolveStair;
            DwonStairrender.material = AfterDissolveDownStair;
            SoundManager.Instance.PlayBGM(SoundManager.GameEvent.Main_2);
            AttachDream.AttachDreamGlobes();
            Dissolved = true;

        }
        
    }
}
