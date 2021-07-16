using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSynth : MonoBehaviour
{
    [SerializeField]
    public Oscilator[] oscilators;

    public float masterVolume = 1f;

    private float[] freq = { 99f, 417f, 432f, 528f, 639f, 852f, 888f };
    private float[] freq4fifths = { 79.2f, 333.6f, 345.6f, 422.4f, 511.2f, 681.6f, 710.4f};

    public void Start()
    {
        Shuffle();
    }

    public void Shuffle()
    {
        oscilators[0].frequency = freq[Random.Range(0, freq.Length - 1)];
        do {
            oscilators[1].frequency = freq[Random.Range(0, freq.Length - 1)];
        } while (oscilators[1].frequency == oscilators[0].frequency);
    }

    public void ProcessAudio(Vector2 baseBeat, Vector2 halfBeat)
    {

        oscilators[0].amplitude = masterVolume * (.8f * Mathf.Abs(Mathf.Pow(baseBeat[0], 6f)));
        oscilators[0].tooth = 1f - Mathf.Abs(baseBeat[0] + .5f * halfBeat[1]);
        oscilators[0].bounce = Mathf.Abs(halfBeat[1] * baseBeat[0]);

        oscilators[1].amplitude = masterVolume * (.8f * Mathf.Abs(baseBeat[1]));
        oscilators[1].tooth = .45f * Mathf.Abs(.8f - Mathf.Pow(halfBeat[0] * baseBeat[1], 1f));
        oscilators[1].bounce = 1f - (Mathf.Abs(halfBeat[0] * baseBeat[1]));

        /*
        for(int i = 0; i < oscilators.Length; i++)
        {
            oscilators[i].amplitude = masterVolume * (ampValues[i].x * Mathf.Abs(Mathf.Pow(baseBeat[bounceValues[i].y], ampValues[i].y)));
            oscilators[i].tooth = toothValues[i].x * Mathf.Abs(toothValues[i].y - baseBeat[bounceValues[i].x]);
            oscilators[i].bounce = Mathf.Abs(halfBeat[bounceValues[i].x] * baseBeat[bounceValues[i].y]);
        }
        */
    }

}
