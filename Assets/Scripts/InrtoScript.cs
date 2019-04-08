using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class InrtoScript : MonoBehaviour
{
    public float delayTime = 2f;
    public TMP_Text titleText;
    public TMP_Text instructionText;
    public Material leftRightMaterial;
    public Material upDownMaterial;
    public Material pressClickMaterial;
    public GameObject trackPad;
    public GameObject trigger;
    public GameObject backButton;
    [SerializeField]
    public Color32[] colors;

    private string[] script = 
    {
        "Press Back - Exit",
        "Swipe Left/Right - Color",
        "Swipe Up/Down - Number",
        "Press Trigger - Reshape",
        "Click Trackpad - Start / Random"

    };
    private string[] titles =
    {
        "WARNING:\nThis software may potentially\ntrigger seizures for people with\nphotosensitive epilepsy.\nViewer discretion is advised.",
        "Psychedelic VR"
    };
    private int stage = -1;
    private int colorIndex = 0;
    private int lineCount = 1;
    private Material originalMaterial;
    private bool titleUp = false;

    // Start is called before the first frame update
    void Start()
    {

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
        titleText.fontSize = 4f;
        originalMaterial = trackPad.GetComponent<MeshRenderer>().sharedMaterial;
        ScriptNext();
        Invoke("ChangeWarningToTitle", delayTime);
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

        if (OVRInput.Get(OVRInput.Button.DpadRight) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            colorIndex = (colorIndex + 1) == colors.Length ? 0 : colorIndex + 1;
            titleText.color = colors[colorIndex];
        }
        if (OVRInput.Get(OVRInput.Button.DpadLeft) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            colorIndex = (colorIndex - 1) == -1 ? colors.Length - 1 : colorIndex - 1;
            titleText.color = colors[colorIndex];
        }
        if (OVRInput.Get(OVRInput.Button.DpadDown) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            lineCount = (lineCount - 1) <= 0 ? 1 : lineCount - 1;
            ChangeTitle(titles[1], lineCount);
        }
        if (OVRInput.Get(OVRInput.Button.DpadUp) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            lineCount = (lineCount + 1) >= 7 ? 6 : lineCount + 1;
            ChangeTitle(titles[1], lineCount);
        }
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            titleText.transform.rotation = (OVRInput.GetLocalControllerRotation(OVRInput.GetActiveController()));
        }
        if (OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad) || Input.GetKeyUp(KeyCode.R))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        }
        if(OVRInput.GetUp(OVRInput.Button.Back) || Input.GetKeyUp(KeyCode.Backspace))
        {
            Application.Quit();
        }
        if (titleUp)
        {
            titleText.outlineWidth = .1f + (Mathf.Sin(Time.time * 10f * Mathf.PI) + 1f) * .05f;
        }
    }

    void ScriptNext()
    {
        if(stage >= script.Length -1)
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
                break;
            case 1:
                backButton.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = leftRightMaterial;
                break;
            case 2:
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = upDownMaterial;
                break;
            case 3:
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
                trigger.GetComponent<MeshRenderer>().sharedMaterial = pressClickMaterial;
                break;
            case 4:
                trigger.GetComponent<MeshRenderer>().sharedMaterial = originalMaterial;
                trackPad.GetComponent<MeshRenderer>().sharedMaterial = pressClickMaterial;
                break;
        }

        Invoke("ScriptNext", delayTime);

    }

    void ChangeTitle(string text, int count)
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

    void ChangeWarningToTitle()
    {
        titleUp = true;
        titleText.text = titles[1];
        titleText.fontSize = 14f;
    }
}
