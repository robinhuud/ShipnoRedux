using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script MUST be called after some script that calls the OVRInput's Update and FixedUpdate methods.
/// use Edit->Preferences->Script Execution Order to change it's execution order.
/// </summary>
public class MatchControllerMotion : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController());
    }
}