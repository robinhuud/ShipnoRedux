using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMaker : MonoBehaviour
{
    public int numLines = 10;
    public int numPoints = 30;
    public Material mat;
    public float noiseScale = .1f;
    public float ringRadius = .5f;

    private NoiseLooper[] noiseLoopers;
    private float noiseOffset = 0f;

    // Start is called before the first frame update
    void Start()
    {
        noiseLoopers = new NoiseLooper[numLines];
        for (int i = 0; i < numLines; i++)
        {
            Vector2 center = new Vector2(Random.Range(5f, 10f), Random.Range(5f,10f));
            Vector2 radii = new Vector2(Random.Range(1f, 1.5f), Random.Range(1f, 1.5f));
            noiseLoopers[i] = new NoiseLooper(center, radii);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for(int i = 0; i < noiseLoopers.Length; i++)
        {
            noiseLoopers[i].SetOffset(.001f, .001f);
        }
    }

    void OnRenderObject()
    {
        DrawLines();
    }

    void DrawLines()
    {
        int l = noiseLoopers.Length;
        float zDist;
        float distScale;
        float x;
        float y;
        float z;
        float noisePos;
        float noiseVal;
        //GL.PushMatrix();
        mat.SetPass(0);
        for (int i = 0; i < l; i++)
        {
            zDist = (float)i / l;
            distScale = 1 - zDist * .8f;
            z = transform.localPosition.z + transform.localScale.z * (zDist - .5f);
            GL.Begin(GL.LINE_STRIP);
            //GL.Color(new Color(1, 1, 1, zDist));
            for (int j = 0; j < numPoints; j++)
            {
                noisePos = (float)j / numPoints;
                noiseVal = noiseLoopers[i].GetLoopedNoise(noisePos + noiseOffset);
                x = transform.localPosition.x + distScale * transform.localScale.x * (noiseScale * noiseVal + ringRadius) * Mathf.Cos(noisePos * Mathf.PI * 2f);
                y = transform.localPosition.y + distScale * transform.localScale.y * (noiseScale * noiseVal + ringRadius) * Mathf.Sin(noisePos * Mathf.PI * 2f);
                GL.TexCoord2(noisePos <= .5f? noisePos * 2f:2f - noisePos * 2f, zDist);
                GL.Vertex3(x, y, z);
            }
            // close the loop
            x = transform.localPosition.x + distScale * transform.localScale.x * (noiseScale * noiseLoopers[i].GetLoopedNoise(noiseOffset) + ringRadius);
            // we know y is zero here, so no need for the math.
            y = transform.localPosition.y;
            GL.TexCoord2(1, zDist);
            GL.Vertex3(x, y, z);
            GL.End();
        }
        //GL.PopMatrix();
    }
}
