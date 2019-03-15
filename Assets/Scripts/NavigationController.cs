using UnityEngine;

public class NavigationController : MonoBehaviour {
    public RibbonGenerator ribbonGenerator;
    public ObjectCloner objectCloner;
    public Oscilator audioSource1;
    public Oscilator audioSource2;

	// Use this for initialization
	void Start () {
        if(OVRManager.display != null)  //OVRManager.display exists or it doesn't, no flag to check other than if it's null.
        {
            OVRManager.display.displayFrequency = 72.0f;
        }
        OVRPlugin.vsyncCount = 0;
    }

    void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }

    void Update ()
    {
        HandleInput();
        ProcessAudio();
    }

    private void HandleInput()
    {

        //Currently handling Oculus GO and keyboard input types
        OVRInput.Update();

        // Back is Quit
        if (OVRInput.GetUp(OVRInput.Button.Back))
        {
            //Debug.Log("GOT BACKBUTTON");
            Application.Quit();
        }
        //Trackpad Gestures on the OVRInput.Get() 
        if (OVRInput.Get(OVRInput.Button.DpadDown) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Debug.Log("GOT DOWN");
            objectCloner.SetNumber(objectCloner.GetNumber() - 1);
        }
        // Up Swipe Gesture (up arrow) increases arm count by 1
        if (OVRInput.Get(OVRInput.Button.DpadUp) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Debug.Log("GOT UP");
            objectCloner.SetNumber(objectCloner.GetNumber() + 1);
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
        if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad) || Input.GetKeyDown(KeyCode.R))
        {
            //Debug.Log("GOT TRACKPAD CLICK");
            ribbonGenerator.RandomizeTime();
            objectCloner.SetNumber(Random.Range(1, 15));
            objectCloner.ChangeColor(Random.Range(-2, 2));
            objectCloner.SetAngle(Quaternion.identity);
        }
        if(OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            //Debug.Log("GOT TRIGGER");
            objectCloner.SetAngle(OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController()));
        }

    }

    private void ProcessAudio()
    {
        // process the audio signals, grab the frequencies from the ribbon, and pass them to the 2 audio sources
        Vector2 frequency = ribbonGenerator.GetFrequency();
        Vector2 amplitude = ribbonGenerator.GetVolume();
        audioSource1.frequency = frequency[0];
        audioSource2.frequency = frequency[1];
        audioSource1.amplitude = amplitude[0];
        audioSource2.amplitude = amplitude[1];
        audioSource1.tooth = 1f - Mathf.Abs(amplitude[0] + .5f) *.666f;
        audioSource2.tooth = 1f - Mathf.Abs(amplitude[1]);
    }

}
