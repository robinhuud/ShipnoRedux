using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script MUST be called after some script that calls the OVRInput's Update and FixedUpdate methods.
/// use Edit->Preferences->Script Execution Order to change it's execution order.
/// </summary>
public class MatchControllerMotion : MonoBehaviour
{
    public OVRPlugin.SystemHeadset headset = 0;
    public OVRInput.Controller currentController = 0;

    private Vector3 offset;

    void Start()
    {
        if(headset == 0)
            headset = OVRPlugin.GetSystemHeadsetType();
        if (currentController == 0)
            currentController = OVRInput.Controller.RTrackedRemote;
        offset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(headset == OVRPlugin.SystemHeadset.Oculus_Go)
        {
            this.transform.rotation = OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController());
        }
        else if(headset == OVRPlugin.SystemHeadset.Oculus_Quest || headset == OVRPlugin.SystemHeadset.Rift_S)
        {
            transform.localPosition = offset+OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote);
            transform.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote);
        }
    }
}