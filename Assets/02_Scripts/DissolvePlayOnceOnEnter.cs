using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using INab.Dissolve;

[RequireComponent(typeof(Dissolver))]
public class DissolvePlayOnceOnEnter : MonoBehaviour
{
    private Dissolver dissolver;
    private bool isPlaying = false;
    private bool hasPlayed = false;

    private void Awake()
    {
        dissolver = GetComponent<Dissolver>();
    }

    private void Update()
    {
        if (!isPlaying && !hasPlayed)
        {
            bool enterPressed =
                Keyboard.current != null &&
                (Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.numpadEnterKey.wasPressedThisFrame);

            if (enterPressed)
            {
                StartCoroutine(PlayOnceRoutine());
            }
        }
    }

    private IEnumerator PlayOnceRoutine()
    {
        isPlaying = true;

        dissolver.Dissolve();
        yield return new WaitForSeconds(dissolver.duration);

        hasPlayed = true;
        isPlaying = false;
    }
}