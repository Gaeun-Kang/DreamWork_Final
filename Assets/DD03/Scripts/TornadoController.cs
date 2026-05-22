using System;
using UnityEngine;
using UnityEngine.VFX;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TornadoController : MonoBehaviour
{
    [SerializeField] private VisualEffect vfx;
    [SerializeField] private float playDuration = 2f;

    private CancellationTokenSource _cts;

    private void Start()
    {
        PlayAndStop();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
    }

    public void PlayAndStop()
    {
        if (vfx == null) return;

        vfx.Play();

        // Cancel any previous running task
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        StopAfterDelay(playDuration, _cts.Token).Forget();
    }

    private async UniTaskVoid StopAfterDelay(float delay, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: token);
            if (!token.IsCancellationRequested)
            {
                vfx.pause = true;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore if canceled
        }
    }
}
