using INab.Dissolve;
using UnityEngine;
using UnityEngine.VFX;

public class MagicBalllVisble : MonoBehaviour
{
    [SerializeField] private VisualEffect magicball;
    [SerializeField] private Dissolver mainDissolver; //기준점이 되는 Dissover script

    //시작할땐 off
    void Awake()
    {
        magicball.enabled = false;

    }

     void OnEnable()
    {
        mainDissolver.OnDissolve += OnMagicballVFX;
    }

    private void OnDisable()
    {
        mainDissolver.OnDissolve -= OnMagicballVFX;
    }


    //MaterialsDissolveValue 값이 0.5 이하일 때
    void OnMagicballVFX(float value)
    {
        if(value < 0.8) magicball.enabled = true;
    }

}
