using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RibbonGenerator : MonoBehaviour {

    public int ribbonLength = 32;
    public float timeScale = 20f;


    // Public interface for getting a sound-scale frequency from the frequency of the ribbon -100-700ish)
    public Vector2 GetFrequency()
    {
        Vector3[] verts = myMeshFilter.mesh.vertices;
        return new Vector2(Mathf.Abs((verts[0].x))*60f+40f, Mathf.Abs((verts[0].y))*60f+49f);
    }

    public Vector2 GetVolume()
    {
        return new Vector2(Mathf.Sin(t)*.15f, Mathf.Cos(t)*.15f);
    }

    private Vector2 scale;
    private Vector2 frequency;
    private Vector2 twirl;
    private float t;
    private Vector2 tempVector;
    private MeshFilter myMeshFilter;

	// Use this for initialization
	void Start () {
        //Debug.Log("Ribbongenerator start");
        Mesh mesh = new Mesh();
        myMeshFilter = GetComponent<MeshFilter>();
        myMeshFilter.mesh = mesh;
        mesh.vertices = GenerateInitialVerts(ribbonLength);
        mesh.uv = GenerateUVs(ribbonLength);
        mesh.triangles = GenerateTriangles(mesh.vertices);
        frequency = new Vector2(.2f, .13f);
        scale = new Vector2(2f, 2f);
        twirl = new Vector2(.2f, .21f);
        t = 50f * (1f / Time.deltaTime * Random.value);
	}

    Vector3[] GenerateInitialVerts(int length)
    {
        Vector3[] newVerts = new Vector3[2 * length];
        for(int i = 0; i < length; i++)
        {
            newVerts[2 * i] = new Vector3(Mathf.Cos((float)i * .2f), Mathf.Sin((float)i * .16f), (length / 4) - (float)i * .5f);
            newVerts[2 * i + 1] = new Vector3(Mathf.Cos((float)i * .2f), Mathf.Sin((float)i * .16f), (length / 4) - (float)i * .5f);
        }
        return newVerts;
    }

    Vector2[] GenerateUVs(int length)
    {
        Vector2[] newUV = new Vector2[2 * length];
        for(int i = 0; i < length; i++)
        {
            newUV[2 * i] = new Vector2(0, (float)i / (float)length);
            newUV[2 * i + 1] = new Vector2(1, (float)i / (float)length);
        }
        return newUV;
    }

    // 3 entries per triangle, 4 triangles per layer (top, bottom, front, back)
    int[] GenerateTriangles(Vector3[] verts)
    {
        int[] newTriangles = new int[3 * (2 * (verts.Length - 2))];
        for (int i = 0; i < verts.Length - 2; i++)
        {
            newTriangles[6 * i] = i;
            newTriangles[6 * i + 1] = i + 1;
            newTriangles[6 * i + 2] = i + 2;
            newTriangles[6 * i + 3] = i;
            newTriangles[6 * i + 4] = i + 2;
            newTriangles[6 * i + 5] = i + 1;
        }
        return newTriangles;
    }

    // Update is called once per frame
    void Update()
    {
        float d = Time.deltaTime;
        t += (timeScale * d) * Mathf.Cos(Time.time * .0237f);
        //Debug.Log("Time is " + (float)t);

        scale += new Vector2(d * .0537f * Mathf.Cos((t * .6f)), d * .0537f * Mathf.Sin((t * .35f)));
        frequency += new Vector2(d * 0.0037f * Mathf.Cos((t * .023f)), d * 0.0039f * Mathf.Sin((t * .032f)));
        twirl += new Vector2(d * .0063f * Mathf.Cos((t * .103f)), d * .0071f * Mathf.Sin((t * .172f)));
        UpdateVertexPositions(myMeshFilter.mesh, t, scale, frequency, twirl);
        UpdateUVPositions(myMeshFilter.mesh, t);
    }

    // update vertex positions copies the x, and y values of each of the verts to the i-2'th vertex in the list, 
    // then generates 2 new x and y positions. the Z-values remain untouched.
    void UpdateVertexPositions(Mesh mesh, float t, Vector2 scale, Vector2 frequency, Vector2 twirl)
    {
        Vector3[] verts = mesh.vertices; // get the old vertex positions
        for(int i = verts.Length - 1; i > 1; i--) // move all the x,y coordinates up 2 places in the queue
        {
            verts[i].x = verts[i - 2].x;
            verts[i].y = verts[i - 2].y;
        }
        // then generate a new x and y position for the first 2 vertices.
        verts[0].x = scale.x * Mathf.Sin((t * frequency.x));
        verts[0].y = scale.y * Mathf.Cos((t * frequency.y));
        //Debug.Log("twirl: " + twirl.x + " " + twirl.y + " freq: " + frequency.x + " " + frequency.y + " scale: " + scale.x + " " + scale.y);
        verts[1].x = verts[0].x + twirl.x * Mathf.Cos(twirl.x * t);
        verts[1].y = verts[0].y + twirl.y * Mathf.Sin(twirl.y * t);
        mesh.vertices = verts;
    }

    void UpdateUVPositions(Mesh mesh, float t)
    {
        Vector2[] uvs = mesh.uv;
        for(int i = uvs.Length - 1; i > 1; i--)
        {
            uvs[i].x = uvs[i - 2].x;
            uvs[i].y = uvs[i - 2].y;
        }
        uvs[0].x = 0.5f * (1 + Mathf.Cos(t));
        uvs[1].x = 0.5f * (1 + Mathf.Sin(t* 1.01f));
        mesh.uv = uvs;
    }

}
