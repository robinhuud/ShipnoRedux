using UnityEngine;
using System.Collections.Generic;

public enum SymmetryType
{
    Rotation,
    Mirror
}

public class ObjectCloner : MonoBehaviour {
    [SerializeField] 
    private GameObject cloneThis;
    [SerializeField]
    private Texture[] greyscaleTextures;
    [SerializeField]
    private Texture[] colorRamps;
    [SerializeField]
    private int numClones = 4;
    [SerializeField]
    private int maxClones = 32;
    [SerializeField]
    private Vector3 axis = Vector3.forward;
    [SerializeField]
    private SymmetryType symmetry = SymmetryType.Rotation;

    // Index into the colorRamp lookup table
    private int colorRampIndex = 0;
    // index into the greyscaleTextures lookup table
    private int greyscaleTextureIndex = 0;
    //Used by the object pooler to manage the ribbons
    private static List<GameObject> ribbonPool;
    // Should we do a re-layout next physics frame?
    private bool dirty = false;
    // All the clones share a material so we only have to manipulate it once
    private Material sharedMat;
    // Store the cycle speed so we can tweak it without asking the shader
    private float cycleSpeed;

    // Use this for initialization
    void Start () {
        Debug.Assert(cloneThis != null, "object cloneThis is not supplied, no object to clone");
        Debug.Assert(cloneThis.GetComponent<MeshFilter>() != null, "Supplied object cloneThis has no mesh renderer");
        Debug.Assert(greyscaleTextures.Length > 0, "No greyscale Texture Array specified");
        Debug.Assert(colorRamps.Length > 0, "No color Ramp Array specified");
        ribbonPool = new List<GameObject>();
        for (int i = 0; i < numClones; i++)
        {
            GameObject clone = CreateRibbon(cloneThis);
            clone.transform.SetParent(this.transform);
        }
        sharedMat = this.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;
        cycleSpeed = Random.Range(-5f, 5f);
        ChangeColor(0);
        dirty = true;
	}
	
	// Update is called once per frame
    // thanks to execution order in Edit->Project Settings->Execution Order we know that this script will run after
    // the RibbonGenerator Update method
	void FixedUpdate () {
        if(dirty)
        {
            ReLayout();
            dirty = false;
        }
    }

    public int GetNumber()
    {
        return numClones;
    }

    public SymmetryType GetSymmetry()
    {
        return symmetry;
    }

    public void SetSymmetry(SymmetryType newSymmetryType)
    {
        if(symmetry != newSymmetryType)
        {
            if(newSymmetryType == SymmetryType.Mirror)
            {
                // When we change to mirror symmetry we force the number of arms to be a power of 2;
                SetNumber(NearestPowerOf2(numClones));
            }
            symmetry = newSymmetryType;
            dirty = true;
        }
    }

    public void SetAngle(Quaternion newAxis)
    {
        //Debug.Log("NEW ANGLE " + newAxis);
        axis = newAxis * Vector3.forward;
        dirty = true;
    }

    public void ChangeCycleSpeed(float delta)
    {
        cycleSpeed += delta;
        sharedMat.SetFloat("_CycleSpeed", cycleSpeed);
    }

    public void ChangeColor(int delta)
    {
        colorRampIndex += delta;
        if(colorRampIndex < 0)
            colorRampIndex += colorRamps.Length;
        else
            colorRampIndex %= colorRamps.Length;
        
        sharedMat.SetTexture("colorMap", colorRamps[colorRampIndex]);
    }
    
    public void ChangeTexture(int delta)
    {
        greyscaleTextureIndex += delta;
        if (greyscaleTextureIndex < 0)
            greyscaleTextureIndex += greyscaleTextures.Length;
        else
            greyscaleTextureIndex %= greyscaleTextures.Length;
        sharedMat.SetTexture("_MainTex", greyscaleTextures[greyscaleTextureIndex]);
    }

    public void SetNumber(int number)
    {
        // restrict number to useful range
        number = Mathf.Clamp(number, 0, maxClones);
        if (symmetry == SymmetryType.Mirror)
        {
            number = NearestPowerOf2(number);
        }

        // If we don't have enough, we get them from the pool
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
        // if we have too many we free them
        else if (number < numClones)
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
        // if it's the same number, no need to do anything
    }

    private int NearestPowerOf2(int number)
    {
        if (number != 1 && number != 2 && number != 4 && number != 8 && number != 16 && number != 32)
        {
            if (number > 25)
                number = 32;
            else if (number > 12)
                number = 16;
            else if (number > 6)
                number = 8;
            else if (number > 2)
                number = 4;
        }
        return number;
    }

    private void ReLayout()
    {
        if(symmetry == SymmetryType.Mirror)
        {
            switch (numClones)
            {
                case 1:
                    this.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(0).localRotation = Quaternion.identity;
                    this.transform.GetChild(0).localPosition = Vector3.zero;
                    break;
                case 2:
                    this.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(0).localRotation = Quaternion.identity;
                    this.transform.GetChild(0).localPosition = Vector3.zero;
                    this.transform.GetChild(1).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(1).localRotation = Quaternion.identity;
                    this.transform.GetChild(1).localPosition = Vector3.zero;
                    break;
                case 4:
                    this.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(0).localRotation = Quaternion.identity;
                    this.transform.GetChild(0).localPosition = Vector3.zero;
                    this.transform.GetChild(1).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(1).localRotation = Quaternion.identity;
                    this.transform.GetChild(1).localPosition = Vector3.zero;
                    this.transform.GetChild(2).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(2).localRotation = Quaternion.identity;
                    this.transform.GetChild(2).localPosition = Vector3.zero;
                    this.transform.GetChild(3).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(3).localRotation = Quaternion.identity;
                    this.transform.GetChild(3).localPosition = Vector3.zero;
                    break;
                case 8:
                    this.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(0).localRotation = Quaternion.identity;
                    this.transform.GetChild(0).localPosition = Vector3.zero;
                    this.transform.GetChild(1).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(1).localRotation = Quaternion.identity;
                    this.transform.GetChild(1).localPosition = Vector3.zero;
                    this.transform.GetChild(2).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(2).localRotation = Quaternion.identity;
                    this.transform.GetChild(2).localPosition = Vector3.zero;
                    this.transform.GetChild(3).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(3).localRotation = Quaternion.identity;
                    this.transform.GetChild(3).localPosition = Vector3.zero;
                    this.transform.GetChild(4).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(4).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(4).localPosition = Vector3.zero;
                    this.transform.GetChild(5).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(5).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(5).localPosition = Vector3.zero;
                    this.transform.GetChild(6).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(6).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(6).localPosition = Vector3.zero;
                    this.transform.GetChild(7).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(7).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(7).localPosition = Vector3.zero;
                    break;
                case 16:
                    this.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(0).localRotation = Quaternion.identity;
                    this.transform.GetChild(0).localPosition = Vector3.zero;
                    this.transform.GetChild(1).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(1).localRotation = Quaternion.identity;
                    this.transform.GetChild(1).localPosition = Vector3.zero;
                    this.transform.GetChild(2).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(2).localRotation = Quaternion.identity;
                    this.transform.GetChild(2).localPosition = Vector3.zero;
                    this.transform.GetChild(3).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(3).localRotation = Quaternion.identity;
                    this.transform.GetChild(3).localPosition = Vector3.zero;
                    this.transform.GetChild(4).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(4).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(4).localPosition = Vector3.zero;
                    this.transform.GetChild(5).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(5).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(5).localPosition = Vector3.zero;
                    this.transform.GetChild(6).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(6).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(6).localPosition = Vector3.zero;
                    this.transform.GetChild(7).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(7).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(7).localPosition = Vector3.zero;
                    this.transform.GetChild(8).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(8).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(8).localPosition = Vector3.zero;
                    this.transform.GetChild(9).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(9).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(9).localPosition = Vector3.zero;
                    this.transform.GetChild(10).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(10).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(10).localPosition = Vector3.zero;
                    this.transform.GetChild(11).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(11).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(11).localPosition = Vector3.zero;
                    this.transform.GetChild(12).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(12).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(12).localPosition = Vector3.zero;
                    this.transform.GetChild(13).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(13).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(13).localPosition = Vector3.zero;
                    this.transform.GetChild(14).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(14).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(14).localPosition = Vector3.zero;
                    this.transform.GetChild(15).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(15).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(15).localPosition = Vector3.zero;
                    break;
                case 32:
                    this.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(0).localRotation = Quaternion.identity;
                    this.transform.GetChild(0).localPosition = Vector3.zero;
                    this.transform.GetChild(1).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(1).localRotation = Quaternion.identity;
                    this.transform.GetChild(1).localPosition = Vector3.zero;
                    this.transform.GetChild(2).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(2).localRotation = Quaternion.identity;
                    this.transform.GetChild(2).localPosition = Vector3.zero;
                    this.transform.GetChild(3).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(3).localRotation = Quaternion.identity;
                    this.transform.GetChild(3).localPosition = Vector3.zero;
                    this.transform.GetChild(4).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(4).localRotation = Quaternion.AngleAxis(22.5f, axis);
                    this.transform.GetChild(4).localPosition = Vector3.zero;
                    this.transform.GetChild(5).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(5).localRotation = Quaternion.AngleAxis(22.5f, axis);
                    this.transform.GetChild(5).localPosition = Vector3.zero;
                    this.transform.GetChild(6).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(6).localRotation = Quaternion.AngleAxis(22.5f, axis);
                    this.transform.GetChild(6).localPosition = Vector3.zero;
                    this.transform.GetChild(7).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(7).localRotation = Quaternion.AngleAxis(22.5f, axis);
                    this.transform.GetChild(7).localPosition = Vector3.zero;
                    this.transform.GetChild(8).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(8).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(8).localPosition = Vector3.zero;
                    this.transform.GetChild(9).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(9).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(9).localPosition = Vector3.zero;
                    this.transform.GetChild(10).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(10).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(10).localPosition = Vector3.zero;
                    this.transform.GetChild(11).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(11).localRotation = Quaternion.AngleAxis(45f, axis);
                    this.transform.GetChild(11).localPosition = Vector3.zero;
                    this.transform.GetChild(12).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(12).localRotation = Quaternion.AngleAxis(67.5f, axis);
                    this.transform.GetChild(12).localPosition = Vector3.zero;
                    this.transform.GetChild(13).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(13).localRotation = Quaternion.AngleAxis(67.5f, axis);
                    this.transform.GetChild(13).localPosition = Vector3.zero;
                    this.transform.GetChild(14).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(14).localRotation = Quaternion.AngleAxis(67.5f, axis);
                    this.transform.GetChild(14).localPosition = Vector3.zero;
                    this.transform.GetChild(15).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(15).localRotation = Quaternion.AngleAxis(67.5f, axis);
                    this.transform.GetChild(15).localPosition = Vector3.zero;
                    this.transform.GetChild(16).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(16).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(16).localPosition = Vector3.zero;
                    this.transform.GetChild(17).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(17).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(17).localPosition = Vector3.zero;
                    this.transform.GetChild(18).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(18).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(18).localPosition = Vector3.zero;
                    this.transform.GetChild(19).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(19).localRotation = Quaternion.AngleAxis(90f, axis);
                    this.transform.GetChild(19).localPosition = Vector3.zero;
                    this.transform.GetChild(20).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(20).localRotation = Quaternion.AngleAxis(112.5f, axis);
                    this.transform.GetChild(20).localPosition = Vector3.zero;
                    this.transform.GetChild(21).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(21).localRotation = Quaternion.AngleAxis(112.5f, axis);
                    this.transform.GetChild(21).localPosition = Vector3.zero;
                    this.transform.GetChild(22).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(22).localRotation = Quaternion.AngleAxis(112.5f, axis);
                    this.transform.GetChild(22).localPosition = Vector3.zero;
                    this.transform.GetChild(23).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(23).localRotation = Quaternion.AngleAxis(112.5f, axis);
                    this.transform.GetChild(23).localPosition = Vector3.zero;
                    this.transform.GetChild(24).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(24).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(24).localPosition = Vector3.zero;
                    this.transform.GetChild(25).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(25).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(25).localPosition = Vector3.zero;
                    this.transform.GetChild(26).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(26).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(26).localPosition = Vector3.zero;
                    this.transform.GetChild(27).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(27).localRotation = Quaternion.AngleAxis(135f, axis);
                    this.transform.GetChild(27).localPosition = Vector3.zero;
                    this.transform.GetChild(28).localScale = new Vector3(1f, 1f, 1f);
                    this.transform.GetChild(28).localRotation = Quaternion.AngleAxis(157.5f, axis);
                    this.transform.GetChild(28).localPosition = Vector3.zero;
                    this.transform.GetChild(29).localScale = new Vector3(-1f, 1f, 1f);
                    this.transform.GetChild(29).localRotation = Quaternion.AngleAxis(157.5f, axis);
                    this.transform.GetChild(29).localPosition = Vector3.zero;
                    this.transform.GetChild(30).localScale = new Vector3(1f, -1f, 1f);
                    this.transform.GetChild(30).localRotation = Quaternion.AngleAxis(157.5f, axis);
                    this.transform.GetChild(30).localPosition = Vector3.zero;
                    this.transform.GetChild(31).localScale = new Vector3(-1f, -1f, 1f);
                    this.transform.GetChild(31).localRotation = Quaternion.AngleAxis(157.5f, axis);
                    this.transform.GetChild(31).localPosition = Vector3.zero;
                    break;
                default:
                    Debug.Log("Symmetry is mirror, but not a power of 2 arms! n=" + numClones);
                    break;
            }
        }
        else
        {
            for (int i = 0; i < numClones; i++)
            {
                this.transform.GetChild(i).localPosition = Vector3.zero;
                this.transform.GetChild(i).localScale = new Vector3(1f, 1f, 1f);
                this.transform.GetChild(i).localRotation = Quaternion.AngleAxis(((float)i / (float)(numClones)) * (360f), axis);
            }
        }
    }

    // This is a little object pooler for the ribbon objects.
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
            target = new GameObject("MeshContainer_"+ribbonPool.Count);
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
