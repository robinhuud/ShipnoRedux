using UnityEngine;
using System.Collections;

public class InputHandler : MonoBehaviour {
    public RibbonGenerator ribbonGenerator;
    public ObjectCloner ribbonCloner;
    public AudioSynth audioSynth;
    public float fadeTime = 3f;

    private bool quitNow = false;
    private bool isStarting = true;
    private Quaternion controllerStartOrientation;
    private Quaternion clonerStartOrientation;

    // Use this for initialization
    void Start () {
        if(audioSynth == null)
        {
            audioSynth = GetComponent<AudioSynth>();
        }
        if(OVRManager.display != null)  //OVRManager.display exists or it doesn't, no flag to check other than if it's null.
        {
            // This does not appear to do anything :(
            OVRManager.display.RecenteredPose += Recenter;
            // Oh yeah, 72Hz mode on Oculus Go, why not right?
            OVRManager.display.displayFrequency = 72.0f;
        }
        OVRPlugin.vsyncCount = 0;
    }
     
    // This along with the OVRManager.display.RecenteredPose += Recenter; in the Start() method
    // is supposed to allow us to be notified when the pose gets recentered, so we can move the
    // major axis back to be lined up with the camera's forward direction.
    // Unfortunately it doesn't work.
    public void Recenter()
    {
        // This never gets called!!!!
        Debug.Log("RecenteredPose event");
        ribbonCloner.SetAngle(Quaternion.identity);
    }

    void FixedUpdate()
    {
        OVRInput.FixedUpdate();
        ProcessAudio();
    }

    void Update ()
    {
        if(isStarting)
        {
            StartCoroutine(StartUp(fadeTime));
            isStarting = false;
        }
        if(quitNow)
        {
            StartCoroutine(StartQuitting(fadeTime));
            quitNow = false;
        }
        OVRInput.Update();
        HandleInput();
    }

    private void HandleInput()
    {
        //Currently handling Oculus GO and keyboard input types

        // Back is Quit
        if (OVRInput.GetUp(OVRInput.Button.Back) || Input.GetKeyUp(KeyCode.Backspace))
        {
            //Debug.Log("GOT BACKBUTTON");
            //audioSynth.masterVolume = 0f;
            quitNow = true;
        }
        //Trackpad Gestures on the OVRInput.Get() 
        if (OVRInput.Get(OVRInput.Button.DpadDown) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            //Debug.Log("GOT DOWN");
            ribbonCloner.SetNumber(ribbonCloner.GetNumber() - 1);
        }
        // Up Swipe Gesture (up arrow) increases arm count by 1
        if (OVRInput.Get(OVRInput.Button.DpadUp) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Debug.Log("GOT UP");
            ribbonCloner.SetNumber(ribbonCloner.GetNumber() + 1);
        }
        if (OVRInput.Get(OVRInput.Button.DpadLeft) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //Debug.Log("GOT LEFT");
            ribbonCloner.ChangeColor(-1);
        }
        if (OVRInput.Get(OVRInput.Button.DpadRight) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            //Debug.Log("GOT RIGHT");
            ribbonCloner.ChangeColor(1);
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad) || Input.GetKeyDown(KeyCode.R))
        {
            //Debug.Log("GOT TRACKPAD CLICK");
            float speedOverride = -1f;
            Quaternion controllerLocalRotation = OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController());
            if(controllerLocalRotation != Quaternion.identity)
            {
                speedOverride = controllerLocalRotation.eulerAngles.z;
                // Convert in to range of 0-1
                speedOverride = 1f - (speedOverride > 180f ? (180f - speedOverride) / 360f: (360f - speedOverride) / 360f + .5f);
            }
            //Debug.Log("ControllerTwist " + controllerTwist);
            ribbonGenerator.RandomizeTime(speedOverride);
            ribbonCloner.transform.rotation = Quaternion.identity;
            ribbonCloner.SetNumber(Random.Range(1, 15));
            ribbonCloner.ChangeColor(Random.Range(-2, 2));
        }
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            //Debug.Log("GOT TRIGGER DOWN");
            controllerStartOrientation = OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController());
            clonerStartOrientation = ribbonCloner.transform.rotation;
            
        } else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            Quaternion rotateBy = Quaternion.Inverse(controllerStartOrientation) * OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController());
            ribbonCloner.transform.rotation = rotateBy * clonerStartOrientation;
        }
    }

    private void ProcessAudio()
    {
        //grab the frequencies and beats from the ribbon, and pass them to the audioSynth
        Vector2 frequencies = ribbonGenerator.GetScaledEndPoint();
        Vector2 baseBeat = ribbonGenerator.GetSinCos();
        Vector2 halfBeat = ribbonGenerator.GetSinCos(2);
        audioSynth.ProcessAudio(frequencies, baseBeat, halfBeat);
    }

    private IEnumerator StartQuitting(float duration)
    {
        float startTime = Time.time;
        float t = startTime;
        while ((t = Time.time) < startTime + duration)
        {
            float amt = 1f - ((t - startTime) / duration);
            audioSynth.masterVolume = amt;
            ribbonCloner.transform.localScale = new Vector3(amt, amt, amt);
            yield return null;
        }
        Application.Quit();
    }

    private IEnumerator StartUp(float duration)
    {
        float startTime = Time.time;
        float t = startTime;
        while((t = Time.time) < startTime + duration)
        {
            float amt = ((t - startTime) / duration);
            audioSynth.masterVolume = amt;
            ribbonCloner.transform.localScale = new Vector3(amt, amt, amt);
            yield return null;
        }
    }
}
