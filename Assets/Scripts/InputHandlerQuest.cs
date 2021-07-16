using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is the version for the Quest / Rift-S, NOT for the Go

public class InputHandlerQuest : MonoBehaviour, ICancelQuit
{
    public RibbonGenerator ribbonGenerator;
    public ObjectCloner ribbonCloner;
    public AudioSynth audioSynth;
    public float fadeTime = 3f;
    public float controllerMovementThreshold = .2f;
    public GameObject quitMenu;
    public GameObject floorObject;

    public float floorHeight = 1.2f;

    private bool quitNow = false;
    private bool isStarting = true;
    private Quaternion controllerStartRotation;
    private Vector3 controllerStartPosition;
    private Quaternion clonerStartRotation;

    // should be an enum? whtever
    // 0 = Unknown (Generic)
    // 2 = Quest
    // 3 = Rift-S
    private short platform = 0;


    // Start is called before the first frame update
    void Start()
    {
        if (audioSynth is null)
        {
            audioSynth = GetComponent<AudioSynth>();
        }
        if (OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Oculus_Go)
        {
            // This version of the script is NOT for the Go
            /*
            platform = 1;
            if (OVRManager.display != null)
            {
                Debug.Log("OVRManager.display is not null (Go)");
                // This does not appear to do anything :(
                OVRManager.display.RecenteredPose += Recenter;
                // Oh yeah, 72Hz mode on Oculus Go, why not right?
                OVRManager.display.displayFrequency = 72.0f;
            }
            */
            Debug.Log("Wrong script for Oculus Go");
        }
        else if (OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Oculus_Quest)
        {
            platform = 2;
            if (OVRManager.display != null)
                Debug.Log("OVRManager.display is not null (Quest)");
            if (OVRManager.boundary != null)
            {
                Vector3[] bounds = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
                Debug.Log("boundary found, floor level is " + bounds[0].y);
                if (bounds.Length > 0) // If the bounds are empty, I do not know how to determine the floor height so we use the default value from the inspector
                    floorObject.transform.SetPositionAndRotation(new Vector3(0, bounds[0].y, 0), Quaternion.identity);
                else
                    floorObject.transform.SetPositionAndRotation(new Vector3(floorObject.transform.position.x, -(floorHeight), floorObject.transform.position.z), Quaternion.identity);
            }
            else
            {
                Debug.Log("boundary is null");
            }
        }
        else if (OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.Rift_S)
        {
            platform = 3;
            if (OVRManager.display != null)
                Debug.Log("OVRManager.display is not null (Rift-S)");
            if (OVRManager.boundary != null)
            {
                Vector3[] bounds = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
                Debug.Log("boundary found, floor level is " + bounds[0].y);
                if (bounds.Length > 0)
                    floorObject.transform.SetPositionAndRotation(new Vector3(0, bounds[0].y, 0), Quaternion.identity);
                else
                    floorObject.transform.SetPositionAndRotation(new Vector3(floorObject.transform.position.x, -(floorHeight), floorObject.transform.position.z), Quaternion.identity);
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
        Debug.Log("OVRPlugin.GetSystemHeadsetType() returns " + OVRPlugin.GetSystemHeadsetType());
        OVRPlugin.vsyncCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarting)
        {
            StartCoroutine(StartUp(fadeTime));
            isStarting = false;
        }
        if (quitNow)
        {
            StartCoroutine(StartQuitting(fadeTime));
            quitNow = false;
        }
        //OVRInput.Update();
        HandleInput();
    }

    void FixedUpdate()
    {
        ProcessAudio();
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

    private void ProcessAudio()
    {
        //grab the beats from the ribbon, and pass them to the audioSynth
        Vector2 baseBeat = ribbonGenerator.GetSinCos();
        Vector2 halfBeat = ribbonGenerator.GetSinCos(2);
        audioSynth.ProcessAudio(baseBeat, halfBeat);
    }

    private void HandleInput()
    {
        Quaternion leftControllerRotation = Quaternion.identity;
        Quaternion rightControllerRotation = Quaternion.identity;
        Vector3 leftControllerPosition = Vector3.zero;
        Vector3 rightControllerPosition = Vector3.zero;
        Vector3 delta;
        // This is ONLY for PC and Quest, separate script for Go
        switch (platform)
        {
            case 0: // Generic
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
                leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTrackedRemote);
                rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote);
                leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTrackedRemote);
                rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote);
                break;
            case 3: // Rift-S
                leftControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTrackedRemote);
                rightControllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote);
                leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTrackedRemote);
                rightControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote);
                break;
        }

        // X-Button (Button.Four) is bottom button on left hand
        // used to switch symmetry
        if(Input.GetKeyUp(KeyCode.X) || OVRInput.GetUp(OVRInput.Button.Four))
        {
            if (ribbonCloner.GetSymmetry() == SymmetryType.Mirror)
            {
                ribbonCloner.SetSymmetry(SymmetryType.Rotation);
            }
            else
            {
                ribbonCloner.SetSymmetry(SymmetryType.Mirror);
            }
        }
        // Controller discreet events, use else if because we don't want to double count clickUp as touchUp
        // Back is Quit
        else if (Input.GetKeyUp(KeyCode.Backspace) || OVRInput.GetUp(OVRInput.Button.Back) || OVRInput.GetUp(OVRInput.Button.Start))
        {
            //Debug.Log("GOT BACKBUTTON");
            //audioSynth.masterVolume = 0f;
            quitNow = true;
        }
        //Trackpad Gestures on the OVRInput.Get() 
        else if (Input.GetKeyUp(KeyCode.DownArrow) || OVRInput.Get(OVRInput.Button.DpadDown) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown))
        {
            //Debug.Log("GOT DOWN");
            if(ribbonCloner.GetSymmetry() == SymmetryType.Rotation)
            {
                ribbonCloner.SetNumber(ribbonCloner.GetNumber() - 1);
            }
            else
            {
                ribbonCloner.SetNumber(ribbonCloner.GetNumber() >> 1);
            }
        }
        // Up Swipe Gesture (up arrow) increases arm count by 1
        else if (Input.GetKeyUp(KeyCode.UpArrow) || OVRInput.Get(OVRInput.Button.DpadUp) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp))
        {
            //Debug.Log("GOT UP");
            if(ribbonCloner.GetSymmetry() == SymmetryType.Rotation)
            {
                ribbonCloner.SetNumber(ribbonCloner.GetNumber() + 1);
            }
            else
            {
                ribbonCloner.SetNumber(ribbonCloner.GetNumber() << 1);
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow) || OVRInput.Get(OVRInput.Button.DpadLeft) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft))
        {
            //Debug.Log("GOT LEFT");
            //ribbonCloner.ChangeColor(-1);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow) || OVRInput.Get(OVRInput.Button.DpadRight) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight))
        {
            //Debug.Log("GOT RIGHT");
            //ribbonCloner.ChangeColor(1);
        }
        else if (Input.GetKeyUp(KeyCode.R) || OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad) || OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick))
        {
            //Debug.Log("GOT TRACKPAD CLICK");
            RandomizeAction();
        }
        else if (OVRInput.Get(OVRInput.Touch.PrimaryTouchpad)) // For GO / Gear
        {
            //touchCoords = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
        }
        else if (OVRInput.Get(OVRInput.Touch.PrimaryThumbstick)) // Quest
        {
            //touchCoords = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        }
        else if (Input.GetKeyUp(KeyCode.S) || OVRInput.GetUp(OVRInput.Touch.PrimaryTouchpad) || OVRInput.GetUp(OVRInput.Touch.PrimaryThumbstick))
        {
            /*
            float speedOverride = -1f;
            float widthOverride = -1f;
            if (hasController) // Only works with controller, no PC control for this.
            {
                //speedOverride = controllerLocalRotation.eulerAngles.z;
                //speedOverride = 1f - (speedOverride > 180f ? (180f - speedOverride) / 360f : (360f - speedOverride) / 360f + .5f);
                //Debug.Log("speedSet " + touchCoords);
                speedOverride = (touchCoords.x + 1f) / 2f;
                widthOverride = (touchCoords.y + 1.2f) / 2.1f;
            }
            ribbonGenerator.SetScaledTime(speedOverride);
            ribbonGenerator.SetWidthOverride(widthOverride);
            */
        }
        // GRAB-TO-ROTATE behavior
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            //When the user pulls the trigger, we remember the orientation of the controller so that we can move relative to the starting orientation
            controllerStartRotation = leftControllerRotation;
            clonerStartRotation = ribbonCloner.transform.rotation;

        }
        else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            //As the trigger is held down we track the relative rotation of the controller and transfer it to the object.
            Quaternion rotateBy = Quaternion.Inverse(controllerStartRotation) * leftControllerRotation;
            ribbonCloner.transform.rotation = rotateBy * clonerStartRotation;
        }
        else
        {
            // slowly drift back to center when not grabbing
            ribbonCloner.transform.rotation = Quaternion.RotateTowards(ribbonCloner.transform.rotation, Quaternion.identity, .05f);
        }

        // New change color gesture is right grab trigger then up/down in space for colormap, left/right for texture, fwd/back for speed
        if(OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            // Record the start position of the right controller so we know how far the user moved.
            controllerStartPosition = rightControllerPosition;
        }
        else if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger))
        {
            delta = rightControllerPosition - controllerStartPosition;
            if (delta.x > controllerMovementThreshold)
            {
                ribbonCloner.ChangeTexture(1);
                controllerStartPosition = new Vector3(rightControllerPosition.x, controllerStartPosition.y, controllerStartPosition.z);
            }
            if (delta.x < -controllerMovementThreshold)
            {
                ribbonCloner.ChangeTexture(-1);
                controllerStartPosition = new Vector3(rightControllerPosition.x, controllerStartPosition.y, controllerStartPosition.z);
            }
            if (delta.y > controllerMovementThreshold)
            {
                ribbonCloner.ChangeColor(1);
                controllerStartPosition = new Vector3(controllerStartPosition.x, rightControllerPosition.y, controllerStartPosition.z);
            }
            if (delta.y < -controllerMovementThreshold)
            {
                ribbonCloner.ChangeColor(-1);
                controllerStartPosition = new Vector3(controllerStartPosition.x, rightControllerPosition.y, controllerStartPosition.z);
            }
            if (delta.z > controllerMovementThreshold)
            {
                ribbonCloner.ChangeCycleSpeed(delta.z);
                controllerStartPosition = new Vector3(controllerStartPosition.x, controllerStartPosition.y, rightControllerPosition.z);
            }
            if (delta.z < -controllerMovementThreshold)
            {
                ribbonCloner.ChangeCycleSpeed(delta.z);
                controllerStartPosition = new Vector3(controllerStartPosition.x, controllerStartPosition.y, rightControllerPosition.z);
            }
        }
    }

    private void RandomizeAction()
    {
        ribbonGenerator.SetScaledTime(-1f);
        ribbonCloner.transform.rotation = Quaternion.identity;
        ribbonCloner.SetNumber(Random.Range(1, 32));
        //ribbonCloner.SetSymmetry(symmetry == SymmetryType.Rotation?SymmetryType.Mirror:SymmetryType.Rotation);
        ribbonCloner.ChangeColor(Random.Range(-2, 2));
        ribbonCloner.ChangeTexture(Random.Range(-2, 2));
        audioSynth.Shuffle();
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
        while ((t = Time.time) < startTime + duration)
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