using UnityEngine;

public class NavigationController : MonoBehaviour {
    public RibbonGenerator ribbonGenerator;
    public ObjectCloner objectCloner;
    public Oscilator audioSource1;
    public Oscilator audioSource2;

	// Use this for initialization
	void Start () {
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
        if (OVRInput.Get(OVRInput.Button.DpadDown) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Debug.Log("GOT DOWN");
            objectCloner.SetNumber(objectCloner.numClones - 1);
        }
        if (OVRInput.Get(OVRInput.Button.DpadUp) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Debug.Log("GOT UP");
            objectCloner.SetNumber(objectCloner.numClones + 1);
        }
        if (OVRInput.Get(OVRInput.Button.DpadLeft) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //Debug.Log("GOT LEFT");
            objectCloner.ChangeColor(-1);
        }
        if (OVRInput.Get(OVRInput.Button.DpadRight) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            //Debug.Log("GOT RIGHT");
            objectCloner.ChangeColor(1);
        }

        // process the audio signals, grab the frequencies from the ribbon, and pass them to the 2 audio sources
        Vector2 frequency = ribbonGenerator.GetFrequency();
        Vector2 amplitude = ribbonGenerator.GetVolume();
        audioSource1.frequency = frequency[0];
        audioSource2.frequency = frequency[1];
        audioSource1.amplitude = amplitude[0];
        audioSource2.amplitude = amplitude[1];
        audioSource1.tooth = (amplitude[1]+.15f)/2f;
        audioSource2.tooth = (amplitude[0]+.15f)/2f;
    }

    void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }
}
