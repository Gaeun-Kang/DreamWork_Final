using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class WPOCollapseLerpController : MonoBehaviour
{
    [Header("Tentacle")]
    [SerializeField] private int tentacleNumber = 1; // 1~5

    [Header("Target")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Shader Properties")]
    [SerializeField] private string collapseLerpProperty = "_WPOCollapseLerp";
    [SerializeField] private string wpoMaxProperty = "_WPOMax";
    [SerializeField] private string wpoIntensityProperty = "_WPOIntensity1";

    [Header("Animation")]
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Collapse Lerp Range")]
    [SerializeField] private float collapseStart = 0f;
    [SerializeField] private float collapseEnd = 1f;

    [Header("WPO Max Range")]
    [SerializeField] private float wpoMaxStart = -0.9f;
    [SerializeField] private float wpoMaxEnd = 0.5f;

    [Header("WPO Intensity Range")]
    [SerializeField] private float wpoIntensityStart = 0.1f;
    [SerializeField] private float wpoIntensityEnd = 0.35f;

    [Header("Behavior")]
    [SerializeField] private bool useInstancedMaterial = true;
    [SerializeField] private bool resetValuesOnStart = true;
    [SerializeField] private bool ignoreIfAlreadyRunning = true;

    private Material runtimeMaterial;
    private Coroutine lerpRoutine;

    private int collapseLerpId;
    private int wpoMaxId;
    private int wpoIntensityId;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        collapseLerpId = Shader.PropertyToID(collapseLerpProperty);
        wpoMaxId = Shader.PropertyToID(wpoMaxProperty);
        wpoIntensityId = Shader.PropertyToID(wpoIntensityProperty);

        runtimeMaterial = useInstancedMaterial
            ? targetRenderer.material
            : targetRenderer.sharedMaterial;

        if (runtimeMaterial == null)
            return;

        if (resetValuesOnStart)
        {
            ApplyStartValues();
        }
    }

    private void Update()
    {
        if (WasMyNumberKeyPressed())
        {
            PlayCollapse();
        }
    }

    private bool WasMyNumberKeyPressed()
    {
        if (Keyboard.current == null)
            return false;

        switch (tentacleNumber)
        {
            case 1: return Keyboard.current.digit1Key.wasPressedThisFrame;
            case 2: return Keyboard.current.digit2Key.wasPressedThisFrame;
            case 3: return Keyboard.current.digit3Key.wasPressedThisFrame;
            case 4: return Keyboard.current.digit4Key.wasPressedThisFrame;
            case 5: return Keyboard.current.digit5Key.wasPressedThisFrame;
            default: return false;
        }
    }

    public void PlayCollapse()
    {
        if (runtimeMaterial == null)
        {
            Debug.LogWarning("Material is missing.", this);
            return;
        }

        if (!runtimeMaterial.HasProperty(collapseLerpId))
        {
            Debug.LogWarning($"Material does not have shader property: {collapseLerpProperty}", this);
            return;
        }

        if (!runtimeMaterial.HasProperty(wpoMaxId))
        {
            Debug.LogWarning($"Material does not have shader property: {wpoMaxProperty}", this);
            return;
        }

        if (!runtimeMaterial.HasProperty(wpoIntensityId))
        {
            Debug.LogWarning($"Material does not have shader property: {wpoIntensityProperty}", this);
            return;
        }

        if (lerpRoutine != null)
        {
            if (ignoreIfAlreadyRunning)
                return;

            StopCoroutine(lerpRoutine);
        }

        lerpRoutine = StartCoroutine(LerpProperties());
    }

    public void ResetCollapse()
    {
        if (runtimeMaterial == null)
            return;

        if (lerpRoutine != null)
        {
            StopCoroutine(lerpRoutine);
            lerpRoutine = null;
        }

        ApplyStartValues();
    }

    private void ApplyStartValues()
    {
        if (runtimeMaterial.HasProperty(collapseLerpId))
            runtimeMaterial.SetFloat(collapseLerpId, collapseStart);

        if (runtimeMaterial.HasProperty(wpoMaxId))
            runtimeMaterial.SetFloat(wpoMaxId, wpoMaxStart);

        if (runtimeMaterial.HasProperty(wpoIntensityId))
            runtimeMaterial.SetFloat(wpoIntensityId, wpoIntensityStart);
    }

    private IEnumerator LerpProperties()
    {
        float time = 0f;

        ApplyStartValues();

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float easedT = curve.Evaluate(t);

            float collapseValue = Mathf.Lerp(collapseStart, collapseEnd, easedT);
            float wpoMaxValue = Mathf.Lerp(wpoMaxStart, wpoMaxEnd, easedT);
            float wpoIntensityValue = Mathf.Lerp(wpoIntensityStart, wpoIntensityEnd, easedT);

            runtimeMaterial.SetFloat(collapseLerpId, collapseValue);
            runtimeMaterial.SetFloat(wpoMaxId, wpoMaxValue);
            runtimeMaterial.SetFloat(wpoIntensityId, wpoIntensityValue);

            yield return null;
        }

        runtimeMaterial.SetFloat(collapseLerpId, collapseEnd);
        runtimeMaterial.SetFloat(wpoMaxId, wpoMaxEnd);
        runtimeMaterial.SetFloat(wpoIntensityId, wpoIntensityEnd);

        lerpRoutine = null;
    }
}