using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSynth : MonoBehaviour
{
    [SerializeField]
    public Oscilator[] oscilators;

    public void ProcessAudio(Vector2 frequencies, Vector2 baseBeat, Vector2 halfBeat)
    {
        oscilators[0].frequency = (1f + Mathf.Abs(frequencies[0])) * 40f + 120f;
        oscilators[0].amplitude = .8f * Mathf.Abs(Mathf.Pow(baseBeat[0], 6f));
        oscilators[0].tooth = 1f - Mathf.Abs(baseBeat[0] + .5f * halfBeat[1]);
        oscilators[0].bounce = Mathf.Abs(halfBeat[1] * baseBeat[0]);

        oscilators[1].frequency = (1f - Mathf.Abs(frequencies[1])) * 40f + 80f;
        oscilators[1].amplitude = .8f * Mathf.Abs(baseBeat[1]);
        oscilators[1].tooth = .45f * Mathf.Abs(.8f - Mathf.Pow(halfBeat[0] * baseBeat[1], 1f));
        oscilators[1].bounce = 1f - (Mathf.Abs(halfBeat[0] * baseBeat[1]));
    }

}
