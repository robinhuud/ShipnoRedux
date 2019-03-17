using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSynth : MonoBehaviour
{
    public Oscilator audioSource1;
    public Oscilator audioSource2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProcessAudio(Vector2 frequencies, Vector2 baseBeat, Vector2 beatFour)
    {
        // Convert -1 - 1 frequency to Audio levels
        audioSource1.frequency = frequencies[0] * 40f + 60f;
        audioSource2.frequency = frequencies[1] * 80f + 120f;
        audioSource1.amplitude = Mathf.Abs(Mathf.Pow(baseBeat[0], 6f));
        audioSource2.amplitude = Mathf.Abs(baseBeat[1] * beatFour[1]);
        audioSource1.tooth = 1f - Mathf.Abs(baseBeat[0] + .5f * beatFour[0]);
        audioSource2.tooth = Mathf.Abs(beatFour[0] * baseBeat[1] * .25f);
        audioSource1.whistle = Mathf.Abs(beatFour[1] * baseBeat[0]);
        audioSource2.whistle = 1f - (Mathf.Abs(beatFour[0] * baseBeat[1]));
    }
}
