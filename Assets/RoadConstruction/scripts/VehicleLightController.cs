using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrafficParticipant;

public class VehicleLightController : MonoBehaviour
{
    Animator m_animator;
    ContinuousLaneController lc;
    private void Awake()
    {
        lc = GetComponentInParent<ContinuousLaneController>();
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        m_animator.SetInteger("laneChangingMove", lc.laneChangingMove);
    }
}
