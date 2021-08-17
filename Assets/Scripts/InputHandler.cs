using UnityEngine;
using System.Collections;

// This is only run on Go platform, the Quest / RiftS version is in InputHandlerQuest.cs

public class InputHandler : MonoBehaviour, ICancelQuit
{
    public RibbonGenerator ribbonGenerator;
    public ObjectCloner ribbonCloner;
    public AudioSynth audioSynth;
    public float fadeTime = 3f;
    public GameObject quitMenu;
    public GameObject floorObject;

    private bool quitNow = false;
    private bool isStarting = true;
    private Quaternion controllerStartRotation;
    private Quaternion clonerStartRotation;
    private Vector2 touchCoords;

    // should be an enum? whtever
    // 0 = PC
    // 1 = Go / GearVR
    // 2 = Quest
    // 3 = Rift-S
    private short platform = 0;

    // Use this for initialization
    void Start () {
        if(audioSynth == null)
        {
            audioSynth = GetComponent<AudioSynth>();
        }
        if(OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Oculus_Go)
        {
            platform = 1;
            if(OVRManager.display != null)
            {
                Debug.Log("OVRManager.display is not null (Go)");
                // This does not appear to do anything :(
                OVRManager.display.RecenteredPose += Recenter;
                // Oh yeah, 72Hz mode on Oculus Go, why not right?
                OVRManager.display.displayFrequency = 72.0f;
            }
        }
        else if(OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Oculus_Quest)
        {
            platform = 2;
            if (OVRManager.display != null)
                Debug.Log("OVRManager.display is not null (Quest)");
            if(OVRManager.boundary != null)
            {
                Vector3[] bounds = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
                if (bounds.Length > 0) // If the bounds are empty, I do not know how to determine the floor height, so we guess at -1.2m (shrug)
                    floorObject.transform.SetPositionAndRotation(new Vector3(0, bounds[0].y, 0), Quaternion.identity);
                else
                    floorObject.transform.SetPositionAndRotation(new Vector3(0, -1.2f, 0), Quaternion.identity);
            }
            else
            {
                Debug.Log("bundary is null");
            }
        }
        else if(OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Rift_S)
        {
            platform = 3;
            if (OVRManager.display != null)
                Debug.Log("OVRManager.display is not null (Rift-S)");
            if (OVRManager.boundary != null)
            {
                Vector3[] bounds = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
                if (bounds.Length > 0)
                    floorObject.transform.SetPositionAndRotation(new Vector3(0, bounds[0].y, 0), Quaternion.identity);
                else
                    floorObject.transform.SetPositionAndRotation(new Vector3(0, -1.2f, 0), Quaternion.identity);
            }
            else
            {
                Debug.Log("boundary is null");
            }
        }
        else
        {
            Debug.Log(" no display, but productName = " + OVRPlugin.productName);
        }
        Debug.Log("OVRPlugin.GetSystemHeadsetType() returns "+ OVRPlugin.GetSystemHeadsetType());
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
        //OVRInput.FixedUpdate();
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
        //OVRInput.Update();
        HandleInput();
    }

    private void HandleInput()
    {
        Quaternion localControllerRotation = Quaternion.identity;
        bool hasController = false;
        //Currently handling Oculus GO, Quest, and keyboard input types
        switch (platform)
        {
            case 0: // PC
                break;
            case 1: // Go
                localControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController());   
                break;
            case 2: // Quest
                    //// controller id strings:
                    //// FUNCTION                       LEFT                RIGHT
                    //// GetLocalControllerRotation     LTrackedRemote      RTrackedRemote
                    //// GetLocalControllerPosition     LTrackedRemote      RTrackedRemote
                    //// Axis2D Thumbstick              LThumbstick         RThumbstick
                    //// Trigger buttons                PrimaryIndexTrigger SecondaryIndexTrigger
                    //// Axis1D Trigger                 PrimaryIndexTrigger SecondaryIndexTrigger
                    //// Neartouch Trigger              PrimaryIndexTrigger SecondaryIndexTrigger
                    //// Grab Buttons                   PrimaryHandTrigger  SecondaryHandTrigger
                    //// Axis1D Hand                    PrimaryHandTrigger  SecondaryHandTrigger
                    //// Neartouch Hand                 PrimaryHandTrigger  SecondaryHandTrigger
                    //// Top button                     Button.Three        Button.One
                    //// bottom button                  Button.Four         Button.Two
                localControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTrackedRemote);
                break;
            case 3: // Rift-S
                localControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTrackedRemote);
                break;
        }
        // Controller discreet events, use else if because we don't want to double count clickUp as touchUp
        // Back is Quit
        if (Input.GetKeyUp(KeyCode.Backspace) || OVRInput.GetUp(OVRInput.Button.Back) || OVRInput.GetUp(OVRInput.Button.Start))
        {
            //Debug.Log("GOT BACKBUTTON");
            //audioSynth.masterVolume = 0f;
            quitNow = true;
        }
        //Trackpad Gestures on the OVRInput.Get() 
        else if (Input.GetKeyUp(KeyCode.DownArrow) || OVRInput.Get(OVRInput.Button.DpadDown) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown))
        {
            //Debug.Log("GOT DOWN");
            ribbonCloner.SetNumber(ribbonCloner.GetNumber() - 1);
        }
        // Up Swipe Gesture (up arrow) increases arm count by 1
        else if (Input.GetKeyUp(KeyCode.UpArrow) || OVRInput.Get(OVRInput.Button.DpadUp) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp))
        {
            //Debug.Log("GOT UP");
            ribbonCloner.SetNumber(ribbonCloner.GetNumber() + 1);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || OVRInput.Get(OVRInput.Button.DpadLeft) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft))
        {
            //Debug.Log("GOT LEFT");
            ribbonCloner.ChangeColor(-1);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow) || OVRInput.Get(OVRInput.Button.DpadRight) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight))
        {
            //Debug.Log("GOT RIGHT");
            ribbonCloner.ChangeColor(1);
        }
        else if (Input.GetKeyUp(KeyCode.R) || OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick))
        {
            //Debug.Log("GOT TRACKPAD CLICK");
            ribbonGenerator.SetScaledTime(-1f);
            ribbonCloner.transform.rotation = Quaternion.identity;
            ribbonCloner.SetNumber(Random.Range(1, 15));
            ribbonCloner.ChangeColor(Random.Range(-2, 2)); 
        }
        else if (OVRInput.Get(OVRInput.Touch.PrimaryTouchpad)) // For GO / Gear
        {
            touchCoords = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
        }
        else if(OVRInput.Get(OVRInput.Touch.PrimaryThumbstick)) // Quest
        {
            touchCoords = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        }
        else if(Input.GetKeyUp(KeyCode.S) || OVRInput.GetUp(OVRInput.Touch.PrimaryTouchpad) || OVRInput.GetUp(OVRInput.Touch.PrimaryThumbstick))
        {
            float speedOverride = -1f;
            float widthOverride = -1f;
            if(hasController) // Only works with controller, no PC control for this.
            {
                //speedOverride = controllerLocalRotation.eulerAngles.z;
                //speedOverride = 1f - (speedOverride > 180f ? (180f - speedOverride) / 360f : (360f - speedOverride) / 360f + .5f);
                //Debug.Log("speedSet " + touchCoords);
                speedOverride = (touchCoords.x + 1f) / 2f;
                widthOverride = (touchCoords.y + 1.2f) / 2.1f;
            }
            ribbonGenerator.SetScaledTime(speedOverride);
            ribbonGenerator.SetWidthOverride(widthOverride);
        }
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            //Debug.Log("GOT TRIGGER DOWN");
            controllerStartRotation = localControllerRotation;
            clonerStartRotation = ribbonCloner.transform.rotation;
            
        } else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            // slowly drift back to center when not grabbing
            Quaternion rotateBy = Quaternion.Inverse(controllerStartRotation) * localControllerRotation;
            ribbonCloner.transform.rotation = rotateBy * clonerStartRotation;
        }
        else
        {
            ribbonCloner.transform.rotation = Quaternion.RotateTowards(ribbonCloner.transform.rotation, Quaternion.identity, .05f);
        }
    }

    private void ProcessAudio()
    {
        //grab the beats from the ribbon, and pass them to the audioSynth
        Vector2 frequencies = ribbonGenerator.GetScaledEndPoint();
        Vector2 baseBeat = ribbonGenerator.GetBeat();
        Vector2 halfBeat = ribbonGenerator.GetBeat(2);
        audioSynth.ProcessAudio(baseBeat, halfBeat);
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
        quitMenu.SetActive(true);
        //Application.Quit();
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

    public void CancelledQuit()
    {
        quitMenu.SetActive(false);
        isStarting = true;
    }
}
