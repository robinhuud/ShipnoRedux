using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseLooper
{
    private Vector2 elipseOrigin;
    private Vector2 elipseRadius;

    public NoiseLooper(Vector2 elipseOrigin, Vector2 elipseRadius)
    {
        this.elipseOrigin = elipseOrigin;
        this.elipseRadius = elipseRadius;
    }

    public float GetLoopedNoise(float value)
    {
        Vector2 elipsePoint = GetElipsePoint(value * Mathf.PI * 2);
        return Mathf.PerlinNoise(elipsePoint.x, elipsePoint.y);
    }

    public void SetOffset(float x, float y)
    {
        elipseOrigin.x += x;
        elipseOrigin.y += y;
    }

    private Vector2 GetElipsePoint(float angle)
    {
        return new Vector2(elipseOrigin.x + Mathf.Cos(angle) * elipseRadius.x, elipseOrigin.y + Mathf.Sin(angle) * elipseRadius.y);
    }
}
