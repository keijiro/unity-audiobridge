using UnityEngine;
using System.Collections;

public class SpectrumBar : MonoBehaviour
{
    public int index;
    public bool useMeanLevel;

    AudioBridge audioBridge;

    void Awake()
    {
        audioBridge = FindObjectOfType(typeof(AudioBridge)) as AudioBridge;
    }

    void Update ()
    {
        var levels = useMeanLevel ? audioBridge.MeanLevels : audioBridge.Levels;
        if (index < levels.Length) {
            var vs = transform.localScale;
            vs.y = 3.0f * (1.0f + levels[index] * 0.01f);
            transform.localScale = vs;
        }
    }
}
