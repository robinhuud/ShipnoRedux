using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class InrtoScript : MonoBehaviour
{
    public float delayTime = 2f;
    public TMP_Text titleText;
    public TMP_Text warningText;
    public TMP_Text instructionText;
    public Material leftRightMaterial;
    public Material upDownMaterial;
    public Material pressClickMaterial;
    public Material multiTapMaterial;
    public GameObject trackPad;
    public GameObject trigger;
    public GameObject backButton;
    public Texture activeColor;
    public Texture selectedColor;
    [SerializeField]
    public Color32[] colors;

    private string[] script =
    {
        "Press Back - Exit",
        "Swipe Left/Right - Color",
        "Swipe Up/Down - Number",
        "Hold Trigger - Reorient",
        "Tap Trackpad - Change Width / Intensity",
        "Click Trackpad - Start / Randomize"
    };
    private string title = "Whirledelic";
    private int stage = -1;
    private int colorIndex = 0;
    private int lineCount = 1;
    private Material originalMaterial;
    private Material currentActiveMaterial;
    private bool titleUp = false;
    private bool isOculusGo;
    private Coroutine nextAction;
    private bool readyToGo = false;
    private bool actionFeedback;
    private Vector2 touchPosition;
    private float outlineScale = .1f;
    private float outlineWiggle = 31f;
    // Start is called before the first frame update
    void Start()
    {
        // Why do I have to put my Effing API key in plain text in the source code? This is terrible security.
        Oculus.Platform.Core.Initialize("2829748573732373");
        Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(
            (Oculus.Platform.Message msg) =>
            {
                if (msg.IsError)
                {
                    Application.Quit();
                }
                else
                {
                    readyToGo = true;
                }
            }
        );
        // must be on the Oculus GO (for now)
        isOculusGo = (OVRPlugin.productName == "Oculus Go");
        if (isOculusGo)
        {
            
        }
        else
        {
            //Application.Quit();
        }

        if (OVRManager.display != null)  //OVRManager.display exists or it doesn't, no flag to check other than if it's null.
        {
            // Oh yeah, 72Hz mode on Oculus Go, why not right?
            OVRManager.display.displayFrequency = 72.0f;
        }
        OVRPlugin.vsyncCount = 0;
        if (titleText == null)
        {
            titleText = GetComponent<TMP_Text>();
        }
        Debug.Assert(titleText != null, "No TextMeshPro object specified or attached");
        Debug.Assert(trackPad != null && trigger != null, "Trackpad and Trigger object not specified");

        titleText.rectTransform.gameObject.SetActive(false);
        warningText.fontSize = 3f;
        warningText.rectTransform.gameObject.SetActive(true);
        originalMaterial = trackPad.GetComponent<MeshRenderer>().sharedMaterial;
        currentActiveMaterial = pressClickMaterial;
        currentActiveMaterial.SetTexture("colorMap", activeColor);
        nextAction = StartCoroutine(ScriptNext(delayTime));
    }

    // Only one script per scene should call these OVRInput methods, but it MUST be before any other scripts
    // that use OVRInput methods in the script execution order (Edit->Preferences->Script Execution Order)
    void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        if(titleUp)
        {
            if (OVRInput.Get(OVRInput.Button.DpadRight) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                colorIndex = (colorIndex + 1) == colors.Length ? 0 : colorIndex + 1;
                UpdateTitleColor();
                actionFeedback = true;
            } else if (OVRInput.Get(OVRInput.Button.DpadLeft) || Input.GetKeyUp(KeyCode.LeftArrow))
            {
                colorIndex = (colorIndex - 1) == -1 ? colors.Length - 1 : colorIndex - 1;
                UpdateTitleColor();
                actionFeedback = true;
            } else if (OVRInput.Get(OVRInput.Button.DpadDown) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                lineCount = (lineCount - 1) <= 0 ? 1 : lineCount - 1;
                UpdateTitleCount(title, lineCount);
                actionFeedback = true;
            } else if(OVRInput.Get(OVRInput.Button.DpadUp) || Input.GetKeyUp(KeyCode.UpArrow))
            {
                lineCount = (lineCount + 1) >= 6 ? 5 : lineCount + 1;
                UpdateTitleCount(title, lineCount);
                actionFeedback = true;
            } else if (OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad) || Input.GetKeyUp(KeyCode.R))
            {
                if(readyToGo)
                {
                    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
                }
            } else if(OVRInput.Get(OVRInput.Touch.PrimaryTouchpad))
            {
                touchPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
            } else if(OVRInput.GetUp(OVRInput.Touch.PrimaryTouchpad) || Input.GetKeyUp(KeyCode.S))
            {
                outlineScale = .03f + (touchPosition.y + 1f) * .05f;
                outlineWiggle = (touchPosition.x + 1f) * 20f;
            }
        } else if (OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad) || Input.GetKeyUp(KeyCode.R))
        {
            if(Time.time < delayTime)
            {
                delayTime = Time.time > 3f ? Time.time : 3f;
                StopCoroutine(nextAction);
                nextAction = StartCoroutine(ScriptNext(delayTime));
            }
            ChangeWarningToTitle();
        }
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            titleText.transform.rotation = (OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController()));
        }
        if(OVRInput.GetUp(OVRInput.Button.Back) || Input.GetKeyUp(KeyCode.Backspace))
        {
            Application.Quit();
        }
        if(actionFeedback)
        {
            StartCoroutine(ActionFeedback());
            actionFeedback = false;
        }
        if (titleUp)
        {
            titleText.outlineWidth = outlineScale * 2f + (Mathf.Sin(Time.time * outlineWiggle) + 1f) * outlineScale;
        }
    }

    IEnumerator ScriptNext(float delayTime)
    {
        if (stage >= script.Length - 1)
        {
            stage = 0;
        }
        else
        {
            stage++;
        }
        instructionText.text = script[stage];
        switch (stage)
        {
            case 0:
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
                backButton.GetComponent<MeshRenderer>().sharedMaterial = pressClickMaterial;
                currentActiveMaterial.SetTexture("colorMap", activeColor);
                currentActiveMaterial = pressClickMaterial;
                break;
            case 1:
                if (!titleUp)
                {
                    ChangeWarningToTitle();
                }
                backButton.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = leftRightMaterial;
                currentActiveMaterial.SetTexture("colorMap", activeColor);
                currentActiveMaterial = leftRightMaterial;
                break;
            case 2:
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = upDownMaterial;
                currentActiveMaterial.SetTexture("colorMap", activeColor);
                currentActiveMaterial = upDownMaterial;
                break;
            case 3:
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
                trigger.GetComponent<MeshRenderer>().sharedMaterial = pressClickMaterial;
                currentActiveMaterial.SetTexture("colorMap", activeColor);
                currentActiveMaterial = pressClickMaterial;
                break;
            case 4:
                trigger.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = multiTapMaterial;
                currentActiveMaterial.SetTexture("colorMap", activeColor);
                currentActiveMaterial = multiTapMaterial;
                break;
            case 5:
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = pressClickMaterial;
                currentActiveMaterial.SetTexture("colorMap", activeColor);
                currentActiveMaterial = pressClickMaterial;
                break;
        }
        yield return new WaitForSeconds(delayTime);
        yield return ScriptNext(delayTime);
    }

    // Used for modifying the Title object's count
    void UpdateTitleCount(string text, int count)
    {
        string builder = "";
        for(int i = 0; i < count; i++)
        {
            builder += text;
            if(i < count - 1)
            {
                builder += "\n";
            }
        }
        titleText.text = builder;
    }

    void UpdateTitleColor()
    {
        titleText.faceColor = colors[colorIndex];
        titleText.outlineColor = new Color32((byte)(colors[colorIndex].r *.35), (byte)(colors[colorIndex].g * .35), (byte)(colors[colorIndex].b * .35), colors[colorIndex].a);
    }

    IEnumerator ActionFeedback()
    {
        Material target = currentActiveMaterial;
        if(target.GetTexture("colorMap") == activeColor)
        {
            target.SetTexture("colorMap", selectedColor);
            yield return new WaitForSeconds(.25f);
            target.SetTexture("colorMap", activeColor);
        }
        yield return null;
    }

    void ChangeWarningToTitle()
    {
        warningText.rectTransform.gameObject.SetActive(false);
        titleUp = true;
        titleText.rectTransform.gameObject.SetActive(true);
        UpdateTitleColor();
    }
}
