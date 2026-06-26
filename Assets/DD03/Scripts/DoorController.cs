using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private List<Animator> m_Animators;

    private bool m_Played;

    private void OnTriggerEnter(Collider col)
    {
        if (m_Played) return;
        m_Played = true;

        foreach (Animator animator in m_Animators)
        {
            animator.SetTrigger("Play");
        }
    }
}