﻿using UnityEngine;

public class NavigationController : MonoBehaviour {
    public RibbonGenerator ribbonGenerator;
    public ObjectCloner objectCloner;
    public Oscilator audioSource1;
    public Oscilator audioSource2;

	// Use this for initialization
	void Start () {
        //Debug.Log("WOOT");
        OVRPlugin.vsyncCount = 0;
    }
	
	// Update is called once per frame
	void Update () {
        OVRInput.Update();

        // not sure why this does not work, the back button must get intercepted on it's way to the event system
        if(OVRInput.Get(OVRInput.Button.Back))
        {
            Debug.Log("GOT A OCULUSGO BACKBUTTON BUTTON PRESS");
            Application.Quit();
        }

        // Controller input for Oculus GO
        if (OVRInput.Get(OVRInput.Button.DpadDown))
        {
            Debug.Log("GOT DOWN");
            objectCloner.setNumber(objectCloner.numClones - 1);
        }
        if (OVRInput.Get(OVRInput.Button.DpadUp))
        {
            Debug.Log("GOT UP");
            objectCloner.setNumber(objectCloner.numClones + 1);
        }

        // Keyboard input for PC
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            objectCloner.setNumber(objectCloner.numClones + 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            objectCloner.setNumber(objectCloner.numClones - 1);
        }

        // process the audio signals, grab the frequencies from the ribbon, and pass them to the 2 audio sources
        Vector2 frequency = ribbonGenerator.GetFrequency();
        Vector2 volume = ribbonGenerator.GetVolume();
        audioSource1.frequency = frequency[0];
        audioSource2.frequency = frequency[1];
        audioSource1.gain = volume[0];
        audioSource2.gain = volume[1];
	}

    void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }
}