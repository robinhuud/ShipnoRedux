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
    void Awake()
    {
        ControllerScript = controller.GetComponent<ICancelQuit>();
        Debug.Assert(ControllerScript != null, "Controller Script does not implement ICancelQuit");
        Debug.Assert(YesAnswer != null, "No YesAnswer object specified");
        Debug.Assert(NoAnswer != null, "No NoAnswer object specified");
        Debug.Assert(ControllerScript != null, "No MyIntroScript object specified");
    }

    // Update is called once per frame
    void Update()
    {
        controllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController()).eulerAngles.y;
        if(controllerDirection < 180)
        {
            if(!currentAnswer)
            {
                popMaker.Play();
            }
            YesAnswer.faceColor = new Color32((byte)255, (byte)255, (byte)255, (byte)255);
            NoAnswer.faceColor = new Color32((byte)128, (byte)128, (byte)128, (byte)255);
            currentAnswer = true;
            if(OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
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
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
            {
                ControllerScript.CancelledQuit();
            }
        }
    }
}