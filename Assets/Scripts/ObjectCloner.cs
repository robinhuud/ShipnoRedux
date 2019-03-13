using UnityEngine;

public class ObjectCloner : MonoBehaviour {
    [SerializeField]
    private GameObject cloneThis;
    [SerializeField]
    private int numClones = 6;
    [SerializeField]
    private int textureIndex = 0;
    public Vector3 axis = Vector3.forward;
    public Vector3 offset = Vector3.zero;
    [SerializeField]
    private Texture[] greyscaleTextures;
    [SerializeField]
    private Texture[] colorRamps;

    private bool dirty = false;

	// Use this for initialization
	void Start () {
        //Debug.Log("Objectcloner start ran");
        GameObject[] clones = new GameObject[numClones];
        Debug.Assert(cloneThis != null, "object cloneThis is not supplied, no object to clone");
        Debug.Assert(cloneThis.GetComponent<MeshFilter>() != null, "Supplied object cloneThis has no mesh renderer");
        Debug.Assert(greyscaleTextures.Length > 0, "No greyscale Texture Array specified");
        Debug.Assert(colorRamps.Length > 0, "No color Ramp Array specified");
        for (int i=0; i<numClones; i++) {
            clones[i] = CreateObject(cloneThis);
            clones[i].transform.SetParent(this.transform);
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

    public void SetNumber(int number)
    {
        if(number > numClones)
        {
            for(int i = 0; i < number - numClones; i++)
            {
                GameObject newClone = CreateObject(cloneThis);
                newClone.transform.SetParent(this.transform);
            }
            dirty = true;
        }
        if(number < numClones)
        {
            if(number >= 1)
            {
                for(int i = 0; i < numClones - number; i++)
                {
                    Destroy(this.transform.GetChild(numClones-(i+1)).gameObject);
                }
                dirty = true;
            }
        }
        if(dirty)
        {
            numClones = number;
            ReLayout();
        }
    }

    public int GetNumber()
    {
        return numClones;
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
        
        this.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", greyscaleTextures[(int)Random.Range(0,greyscaleTextures.Length-1)]);
        this.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.SetTexture("colorMap", colorRamps[textureIndex]);
    }

    private void ReLayout()
    {
        //Debug.Log("Relayout to " + numClones);
        for (int i = 0; i < numClones; i++)
        {
            this.transform.GetChild(i).SetPositionAndRotation(offset, Quaternion.AngleAxis(((float)i / (float)(numClones)) * (360f), axis));
        }
        dirty = false;
    }

    private GameObject CreateObject(GameObject cloneThis)
    {
        GameObject newGameObject = new GameObject("SwirlArm");
        newGameObject.AddComponent<MeshFilter>().sharedMesh = cloneThis.GetComponent<MeshFilter>().mesh;
        newGameObject.AddComponent<MeshRenderer>().sharedMaterial = cloneThis.GetComponent<MeshRenderer>().material;
        return newGameObject;
    }
}
