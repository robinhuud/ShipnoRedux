using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscilator: MonoBehaviour {

    // Interface for this thing is just to change the 4 public variables,

    public double frequency = 440.0;
    public float amplitude = 0;
    public float bounce = 0; // used as a LERP key, so values outside 0-1 get clamped
    public float tooth = 0; // used as a LERP key, so values outside 0-1 get clamped

    private double increment;
    private double phase;
    private double sampling_frequency = 48000.0;
    private float sin;
    private float saw;
    private float hump;

    public void OnAudioFilterRead(float[] data, int channels)
    {
        increment = frequency * 2.0 * Mathf.PI / sampling_frequency;

        for(int i = 0; i < data.Length; i+= channels)
        {
            phase += increment;
            sin = amplitude * Mathf.Sin((float)phase); // PHASE of 2*PI MUST be the same for all wave shapes
            saw = amplitude - ((amplitude / (Mathf.PI)) * ((float)phase));
            hump = Mathf.Abs(sin);
            sin = Mathf.Lerp(sin, hump, bounce);
            data[i] = Mathf.Lerp(sin, saw, tooth);

            if(channels == 2)
            {
                data[i + 1] = data[i];
            }

            phase %= Mathf.PI * 2.0;
        }
    }
}
