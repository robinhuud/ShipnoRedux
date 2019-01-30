using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscilator : MonoBehaviour {

    public double frequency = 440.0;
    public float amplitude;
    public float tooth = 0;

    private double increment;
    private double phase;
    private double sampling_frequency = 48000.0;
    private float sin;
    private float saw;

    void OnAudioFilterRead(float[] data, int channels)
    {
        increment = frequency * 2.0 * Mathf.PI / sampling_frequency;

        for(int i = 0; i < data.Length; i+= channels)
        {
            phase += increment;
            sin = amplitude * Mathf.Sin((float)phase);
            saw = amplitude - (amplitude / Mathf.PI * (float)phase);
            data[i] = Mathf.Lerp(sin, saw, tooth);

            if(channels == 2)
            {
                data[i + 1] = data[i];
            }

            phase %= Mathf.PI * 2.0;
        }
    }
}
