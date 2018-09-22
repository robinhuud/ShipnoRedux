using UnityEngine;

public class ObjectCloner : MonoBehaviour {
    public GameObject cloneThis;
    public int numClones = 6;
    public Vector3 axis = Vector3.forward;
    public Vector3 offset = Vector3.zero;

    private bool dirty = false;

	// Use this for initialization
	void Start () {
        //Debug.Log("Objectcloner start ran");
        GameObject[] clones = new GameObject[numClones];
        Debug.Assert(cloneThis != null, "object cloneThis is not supplied, no object to clone");
        Debug.Assert(cloneThis.GetComponent<MeshFilter>() != null, "Supplied object cloneThis has no mesh renderer");
        for(int i=0; i<numClones; i++) {
            clones[i] = createObject(cloneThis);
            clones[i].transform.SetParent(this.transform);
        }
        dirty = true;
	}
	
	// Update is called once per frame
    // thanks to execution order in Edit->Project Settings->Execution Order we know that this script will run after
    // the RibbonGenerator Update method
	void Update () {
        if(dirty)
        {
            reLayout();
        }
    }

    public void setNumber(int number)
    {
        if(number > numClones)
        {
            for(int i = 0; i < number - numClones; i++)
            {
                GameObject newClone = createObject(cloneThis);
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
            reLayout();
        }
    }

    void reLayout()
    {
        Debug.Log("Relayout to " + numClones);
        for (int i = 0; i < numClones; i++)
        {
            this.transform.GetChild(i).SetPositionAndRotation(offset, Quaternion.AngleAxis(((float)i / (float)(numClones)) * (360f), axis));
        }
        dirty = false;
    }

    GameObject createObject(GameObject cloneThis)
    {
        GameObject newGameObject = new GameObject("SwirlArm");
        newGameObject.AddComponent<MeshFilter>().sharedMesh = cloneThis.GetComponent<MeshFilter>().mesh;
        newGameObject.AddComponent<MeshRenderer>().sharedMaterial = cloneThis.GetComponent<MeshRenderer>().material;
        return newGameObject;
    }
}
