using INab.Dissolve;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.ParticleSystem;

public class MagicBalllVisble : MonoBehaviour
{
    [SerializeField] private VisualEffect magicball;
    [SerializeField] private Dissolver mainDissolver; //기준점이 되는 Dissover script
     public string BallScale = "BallScale";
     private float duration = 1.2f;
    

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

    public void ShrinkVFX()
    {
        magicball.SetFloat(BallScale, 0f);
    }
}
