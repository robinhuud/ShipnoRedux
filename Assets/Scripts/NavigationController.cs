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
            // Oh yeah, 72Hz mode on Oculus Go, why not right?
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
        Vector2 frequencies = ribbonGenerator.GetAudioFrequencies();
        Vector2 baseBeat = ribbonGenerator.GetSinCos();
        Vector2 beatFour = ribbonGenerator.GetSinCos(4);
        audioSource1.frequency = frequencies[0] * 40f + 60f;
        audioSource2.frequency = frequencies[1] * 80f + 120f;
        audioSource1.amplitude = Mathf.Abs(Mathf.Pow(baseBeat[0],6f));
        audioSource2.amplitude = Mathf.Abs(baseBeat[1] * beatFour[1] * .5f);
        audioSource1.tooth = 1f - Mathf.Abs(baseBeat[0] + .5f);
        audioSource2.tooth = Mathf.Abs(beatFour[0] * baseBeat[1] * .25f);
    }

}
