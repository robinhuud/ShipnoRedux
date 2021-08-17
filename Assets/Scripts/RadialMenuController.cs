using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject MenuBase;
    [SerializeField]
    private GameObject MenuHighlight;
    [SerializeField]
    private Transform RightHandAnchor;
    [SerializeField]
    private Transform LeftHandAnchor;

    public float deadZoneSize = .1f;

    private bool iAmActive = false;
    private Vector3 startAnchor;
    private Transform activeAnchor;
    private Vector3 delta;
    private Vector2 delta2;
    private float angle;
    private int selection = -3;

    public int GetItemId()
    {
        return (selection + 2) % 4;
    }

    private void Awake()
    {
        MenuBase.SetActive(false);
        MenuHighlight.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        iAmActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (iAmActive)
        {
            delta = activeAnchor.position - startAnchor;
            delta2 = new Vector2(delta.x, delta.y);
            angle = Vector2.Angle(Vector2.right, delta2);
            if (delta2.y < 0)
                angle = - angle;

            if (delta2.magnitude > deadZoneSize)
            {
                MenuHighlight.SetActive(true);
                selection = Mathf.RoundToInt(angle / 90);
                angle = selection*90f;
                MenuHighlight.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            else
            {
                selection = -3;
                MenuHighlight.SetActive(false);
            }
        }

    }
    private void OnRenderObject()
    {
        if(iAmActive)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.Vertex(startAnchor);
            GL.Vertex(activeAnchor.position);
            GL.End();
        }
    }

    public void Activate(bool isRightHand)
    {
        MenuBase.SetActive(true);
        MenuHighlight.SetActive(false);
        if (isRightHand)
        {
            MenuBase.transform.position = RightHandAnchor.position;
            activeAnchor = RightHandAnchor;
        }
        else
        {
            MenuBase.transform.position = LeftHandAnchor.position;
            activeAnchor = LeftHandAnchor;
        }
        startAnchor = MenuBase.transform.position;
        iAmActive = true;
    }

    public void Deactivate()
    {
        iAmActive = false;
        MenuBase.SetActive(false);
        MenuHighlight.SetActive(false);
    }
}
