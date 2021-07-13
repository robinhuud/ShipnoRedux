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
    public Color32 activeColor = new Color32((byte)255, (byte)255, (byte)255, (byte)255);
    public Color32 inactiveColor = new Color32((byte)128, (byte)128, (byte)128, (byte)255);

    private ICancelQuit ControllerScript;
    private float controllerDirection;
    private bool currentAnswer = false;
    private short controlType = 0; // 0- PC, 1-Go, 2-Quest
    void Awake()
    {
        ControllerScript = controller.GetComponent<ICancelQuit>();
        Debug.Assert(!(ControllerScript is null), "Controller Script does not implement ICancelQuit");
        Debug.Assert(!(YesAnswer is null), "No YesAnswer object specified");
        Debug.Assert(!(NoAnswer is null), "No NoAnswer object specified");
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
            case 0: // PC
                controllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote).eulerAngles.y;
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
            YesAnswer.faceColor = activeColor;
            NoAnswer.faceColor = inactiveColor;
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
            NoAnswer.faceColor = activeColor;
            YesAnswer.faceColor = inactiveColor;
            currentAnswer = false;
            if (Input.GetKeyUp(KeyCode.N) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                ControllerScript.CancelledQuit();
            }
        }
    }
}