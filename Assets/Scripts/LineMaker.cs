using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMaker : MonoBehaviour
{
    public int numLines = 10;
    public int numPoints = 30;
    public Material mat;

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
    void Update()
    {
        for(int i = 0; i < noiseLoopers.Length; i++)
        {
            noiseLoopers[i].SetOffset(.01f, .01f);
        }
    }

    void OnRenderObject()
    {
        DrawLines();
    }

    void DrawLines()
    {
        float alpha = 0;
        GL.PushMatrix();
        mat.SetPass(0);
        for (int i = 0; i < noiseLoopers.Length; i++)
        {
            float x;
            float y;
            float z = transform.localPosition.z + transform.localScale.z * (((float)i / (float)noiseLoopers.Length) - .5f);
            float noisePos = 0;
            alpha = i / noiseLoopers.Length;
            GL.Begin(GL.LINE_STRIP);
            GL.Color(new Color(1, 1, 1, alpha));
            for (int j = 0; j < numPoints; j++)
            {
                noisePos = (float)j / (float)numPoints;
                x = transform.localPosition.x + transform.localScale.x * (.5f * noiseLoopers[i].GetLoopedNoise(noisePos + noiseOffset) + .5f) * Mathf.Cos(noisePos * Mathf.PI * 2f);
                y = transform.localPosition.y + transform.localScale.y * (.5f * noiseLoopers[i].GetLoopedNoise(noisePos + noiseOffset) + .5f) * Mathf.Sin(noisePos * Mathf.PI * 2f);
                GL.TexCoord2(noisePos, ((float)i / (float)noiseLoopers.Length));
                GL.Vertex3(x, y, z);
            }
            // close the loop
            x = transform.localPosition.x + transform.localScale.x * (.5f * noiseLoopers[i].GetLoopedNoise(noiseOffset) + .5f);
            y = transform.localPosition.y;
            GL.TexCoord2(1, ((float)i / (float)noiseLoopers.Length));
            GL.Vertex3(x, y, z);
            GL.End();
        }
        GL.PopMatrix();
    }
}
