using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Thia script is dependant on another script (before it in execution order) 
/// calling all of the Oculus OVRInput initialization stuff.
/// </summary>
public class QuitNowBehavior : MonoBehaviour
{
    public AudioSource popMaker;
    public TMP_Text YesAnswer;
    public TMP_Text NoAnswer;
    public GameObject controller;

    private ICancelQuit ControllerScript;
    private float controllerDirection;
    private bool currentAnswer = false;
    private short controlType = 0; // 0- PC, 1-Go, 2-Quest
    void Awake()
    {
        ControllerScript = controller.GetComponent<ICancelQuit>();
        Debug.Assert(ControllerScript != null, "Controller Script does not implement ICancelQuit");
        Debug.Assert(YesAnswer != null, "No YesAnswer object specified");
        Debug.Assert(NoAnswer != null, "No NoAnswer object specified");
        if (OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Oculus_Go)
        {
            controlType = 1;
        }
        else if (OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Oculus_Quest)
        {
            controlType = 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(controlType)
        {
            case 0:
                break;
            case 1: // Go
                controllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController()).eulerAngles.y;
                break;
            case 2: // Quest
                controllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote).eulerAngles.y;
                break;
        }
        
        if(controllerDirection < 180)
        {
            if(!currentAnswer)
            {
                popMaker.Play();
            }
            YesAnswer.faceColor = new Color32((byte)255, (byte)255, (byte)255, (byte)255);
            NoAnswer.faceColor = new Color32((byte)128, (byte)128, (byte)128, (byte)255);
            currentAnswer = true;
            if(Input.GetKeyUp(KeyCode.Y) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                Application.Quit();
            }
        }
        else
        {
            if (currentAnswer)
            {
                popMaker.Play();
            }
            NoAnswer.faceColor = new Color32((byte)255, (byte)255, (byte)255, (byte)255);
            YesAnswer.faceColor = new Color32((byte)128, (byte)128, (byte)128, (byte)255);
            currentAnswer = false;
            if (Input.GetKeyUp(KeyCode.N) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                ControllerScript.CancelledQuit();
            }
        }
    }
}