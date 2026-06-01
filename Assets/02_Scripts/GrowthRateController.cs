using UnityEngine;
using System.Collections;
using System;

public class GrowthRateController : MonoBehaviour
{
    public Renderer targetRenderer;
    public string growthRateProperty = "_Growth_Rate";
    public float duration = 2f;

    [SerializeField] private Material mat;
    [SerializeField] private Coroutine routine;


    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        mat = targetRenderer.material;

        if (mat.HasProperty(growthRateProperty))
        {
            mat.SetFloat(growthRateProperty, 0f);
        }
        else
        {
            Debug.LogError($"Material does not have property: {growthRateProperty}");
        }
    }

    private void OnEnable()
    {
        DreamSphereManager.Instance.OnSpherehover += HandleRaySelect;
    }

    private void HandleRaySelect(GameObject gameobject)
    {
        Debug.Log("오브젝트 선택 확인");
        PlayGrowth();
    }


    public void PlayGrowth()
    {
        if (mat == null) return;

        if (!mat.HasProperty(growthRateProperty))
        {
            Debug.LogError($"Material does not have property: {growthRateProperty}");
            return;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ChangeGrowthRate(0f, 1f));
        //한번만 하고 이벤트 해제!
        DreamSphereManager.Instance.OnSpherehover -= HandleRaySelect;
    }

    public void ResetGrowth()
    {
        if (mat == null) return;

        if (routine != null)
            StopCoroutine(routine);

        if (mat.HasProperty(growthRateProperty))
            mat.SetFloat(growthRateProperty, 0f);
    }

    IEnumerator ChangeGrowthRate(float from, float to)
    {
        float elapsed = 0f;
        mat.SetFloat(growthRateProperty, from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float value = Mathf.Lerp(from, to, t);
            mat.SetFloat(growthRateProperty, value);

            yield return null;
        }

        mat.SetFloat(growthRateProperty, to);
        routine = null;

      

    }
}