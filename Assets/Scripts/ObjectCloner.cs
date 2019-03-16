using UnityEngine;
using System.Collections.Generic;

public class ObjectCloner : MonoBehaviour {
    //This Serializable private field pattern is good C# form, but causes compiler warnings in Unity, if you want to get rid of the warning, make them public
    [SerializeField] 
    private GameObject cloneThis;
    [SerializeField]
    private Texture[] greyscaleTextures;
    [SerializeField]
    private Texture[] colorRamps;
    [SerializeField]
    private int numClones = 6;
    [SerializeField]
    private int maxClones = 30;
    [SerializeField]
    private Vector3 axis = Vector3.forward;

    private int textureIndex = 0;
    private static List<GameObject> ribbonPool;
    private bool dirty = false;

	// Use this for initialization
	void Start () {
        Debug.Assert(cloneThis != null, "object cloneThis is not supplied, no object to clone");
        Debug.Assert(cloneThis.GetComponent<MeshFilter>() != null, "Supplied object cloneThis has no mesh renderer");
        Debug.Assert(greyscaleTextures.Length > 0, "No greyscale Texture Array specified");
        Debug.Assert(colorRamps.Length > 0, "No color Ramp Array specified");
        ribbonPool = new List<GameObject>();
        for (int i=0; i<numClones; i++) {
            GameObject clone = CreateRibbon(cloneThis);
            clone.transform.SetParent(this.transform);
        }
        ChangeColor(0);
        dirty = true;
	}
	
	// Update is called once per frame
    // thanks to execution order in Edit->Project Settings->Execution Order we know that this script will run after
    // the RibbonGenerator Update method
	void Update () {
        if(dirty)
        {
            ReLayout();
        }
    }

    public int GetNumber()
    {
        return numClones;
    }

    public void SetAngle(Quaternion newAxis)
    {
        //Debug.Log("NEW ANGLE " + newAxis);
        axis = newAxis * Vector3.forward;
        dirty = true;
    }

    public void ChangeColor(int delta)
    {
        textureIndex += delta;
        if(textureIndex < 0)
        {
            textureIndex += colorRamps.Length;
        }
        else
        {
            textureIndex = textureIndex % colorRamps.Length;
        }
        
        this.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", greyscaleTextures[(int)Random.Range(0,greyscaleTextures.Length)]);
        this.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("colorMap", colorRamps[textureIndex]);
    }

    public void SetNumber(int number)
    {
        number = Mathf.Min(number, maxClones);
        if (number > numClones)
        {
            for (int i = 0; i < number - numClones; i++)
            {
                GameObject newClone = CreateRibbon(cloneThis);
                newClone.transform.SetParent(this.transform);
            }
            numClones = number;
            dirty = true;
        }
        if (number < numClones)
        {
            if (number >= 1)
            {
                for (int i = 0; i < numClones - number; i++)
                {
                    FreeRibbon(this.transform.GetChild(numClones - (i + 1)).gameObject);
                }
                numClones = number;
                dirty = true;
            }
        }
    }

    private void ReLayout()
    {
        for (int i = 0; i < numClones; i++)
        {
            this.transform.GetChild(i).SetPositionAndRotation(Vector3.zero, Quaternion.AngleAxis(((float)i / (float)(numClones)) * (360f), axis));
        }
        dirty = false;
    }

    private static GameObject CreateRibbon(GameObject cloneThis)
    {
        GameObject target = null;
        foreach(GameObject ob in ribbonPool)
        {
            if(!ob.activeInHierarchy)
            {
                // we find the first inactive object in the pool, and activate it
                ob.SetActive(true);
                target = ob;
                break;
            }
        }
        if(target == null)
        {
            target = new GameObject("SwirlArm");
            ribbonPool.Add(target);
            target.AddComponent<MeshFilter>().sharedMesh = cloneThis.GetComponent<MeshFilter>().mesh;
            target.AddComponent<MeshRenderer>().sharedMaterial = cloneThis.GetComponent<MeshRenderer>().material;
        }
        return target;
    }

    private static void FreeRibbon(GameObject target)
    {
        target.SetActive(false);
    }
}
