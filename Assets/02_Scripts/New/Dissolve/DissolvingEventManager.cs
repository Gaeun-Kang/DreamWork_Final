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

    //Dissolve동안 일어나는 이벤트
    //1. Material 교체   2. PlayableDirector(Morph Event)

    private void DissolvingEvent(float value)
    {

        if (value < 1.4f)
        {
            playableDirector.time += Time.deltaTime;
            playableDirector.Evaluate();
        }

        if (value < 0.8f) MorphBaker.enabled = false;

        if (value < 0.5f && Dissolved == false) {

           // vignette.active = true; 
            DomeMeshrenderer.material = AfterDissolve;
            playableDirector.Stop();
            AttachDream.AttachDreamGlobes();
            Dissolved = true;

        }
        
    }
}
