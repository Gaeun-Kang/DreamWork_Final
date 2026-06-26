using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigRef : Singleton<PlayerRigRef>
{
    [SerializeField] private Transform m_CenterEyeAnchor;
    [SerializeField] private OVRHand m_LeftHand;
    [SerializeField] private OVRHand m_RightHand;

    public Transform CenterEyeAnchor => m_CenterEyeAnchor;
    public OVRHand LeftHand => m_LeftHand;
    public OVRHand RightHand => m_RightHand;
}
