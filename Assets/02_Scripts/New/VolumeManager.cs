using INab.Dissolve;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private Dissolver mainDissolver;
    private Vignette Vignette;

    private void OnEnable()
    {
        mainDissolver.OnDissolve += OnVignette;
    }

    private void OnDisable()
    {
        mainDissolver.OnDissolve -= OnVignette;
    }

    private void OnVignette(float value)
    {
        if (value < 1.4f) Vignette.active = true;
    }
}
